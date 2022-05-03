using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.UIControls;

using Kentico.Xperience.Twilio.SendGrid.Models;

using Newtonsoft.Json;

using SendGrid;

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Kentico.Xperience.Twilio.SendGrid.Pages
{
    public partial class BounceManagement : CMSPage
    {
        private IEnumerable<SendGridBounce> bounceData;


        private BaseInfo EditedNewsletterOrIssue
        {
            get
            {
                var objectId = QueryHelper.GetInteger("objectid", 0);
                var parentId = QueryHelper.GetInteger("parentobjectid", 0);
                if (parentId > 0)
                {
                    // We are editing an issue of a campaign
                    return IssueInfo.Provider.Get(objectId);
                }
                
                // We are editing a newsletter
                return NewsletterInfo.Provider.Get(objectId);
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            LoadBounceData();
            LoadGridData();

            gridReport.OnExternalDataBound += gridReport_OnExternalDataBound;
        }


        private object gridReport_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            switch (sourceName)
            {
                case "sg-bounced":
                    if (bounceData == null)
                    {
                        return String.Empty;
                    }

                    var email = ValidationHelper.GetString(parameter, String.Empty);
                    return UniGridFunctions.ColoredSpanYesNo(bounceData.Any(b => b.Email == email));
                case "kx-bounced":
                    var drv = parameter as DataRowView;
                    var settingsService = Service.Resolve<ISettingsService>();
                    var contactBounces = ValidationHelper.GetInteger(drv[nameof(ContactInfo.ContactBounces)], 0);
                    var bounceLimit = ValidationHelper.GetInteger(settingsService["CMSBouncedEmailsLimit"], 0);
                    return UniGridFunctions.ColoredSpanYesNo(contactBounces >= bounceLimit);
            }

            return parameter;
        }


        private void LoadBounceData()
        {
            var sendGridClient = Service.ResolveOptional<ISendGridClient>();
            if (sendGridClient == null)
            {
                ShowError("The SendGrid client is not configured properly.");
                return;
            }

            var response = sendGridClient.RequestAsync(
                method: BaseClient.Method.GET,
                urlPath: "suppression/bounces"
            ).ConfigureAwait(false).GetAwaiter().GetResult();
            var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                bounceData = JsonConvert.DeserializeObject<IEnumerable<SendGridBounce>>(responseBody);
                return;
            }

            gridReport.Visible = false;
            gridReport.StopProcessing = true;

            var responseError = JsonConvert.DeserializeObject<SendGridApiErrorResponse>(responseBody);
            var errorDescriptions = responseError.Errors.Select(err =>
            {
                var prefix = String.IsNullOrEmpty(err.Field) ? String.Empty : $"Field \"{err.Field}\": ";
                return $"- {prefix}\"{err.Message}\"";
            });
            var logDescription = $"Unable to load bounces from SendGrid:\r\n\r\n{String.Join("\r\n", errorDescriptions)}";
            var eventLogService = Service.Resolve<IEventLogService>();

            eventLogService.LogError(nameof(BounceManagement), nameof(LoadBounceData), logDescription);
            ShowError("Unable to load bounces from SendGrid. Please check the Event Log.");
        }


        private void LoadGridData()
        {
            if (gridReport.StopProcessing)
            {
                return;
            }

            InfoDataSet<ContactInfo> subscribedContacts = null;
            if (EditedNewsletterOrIssue is NewsletterInfo)
            {
                subscribedContacts = LoadNewsletterSubscribers(EditedNewsletterOrIssue as NewsletterInfo);
            }
            else if (EditedNewsletterOrIssue is IssueInfo)
            {
                subscribedContacts = LoadCampaignIssueSubscribers(EditedNewsletterOrIssue as IssueInfo);
            }

            if (subscribedContacts != null)
            {
                gridReport.DataSource = subscribedContacts;
                gridReport.DataBind();
            }
        }


        private InfoDataSet<ContactInfo> LoadCampaignIssueSubscribers(IssueInfo issue)
        {
            var contactsWithEmail = ContactInfo.Provider.Get().WhereNotEmpty(nameof(ContactInfo.ContactEmail));
            var contactGroupIds = IssueContactGroupInfo.Provider.Get()
                .Column(nameof(IssueContactGroupInfo.ContactGroupID))
                .WhereEquals(nameof(IssueContactGroupInfo.IssueID), issue.IssueID);
            var contactGroupMemberIds = ContactGroupMemberInfo.Provider.Get()
                .Column(nameof(ContactGroupMemberInfo.ContactGroupMemberRelatedID))
                .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberType), (int)ContactGroupMemberTypeEnum.Contact)
                .WhereIn(nameof(ContactGroupMemberInfo.ContactGroupMemberContactGroupID), contactGroupIds);
            return contactsWithEmail.Where(where => where
                .WhereIn(nameof(ContactInfo.ContactID), contactGroupMemberIds)
            ).TypedResult;
        }


        private InfoDataSet<ContactInfo> LoadNewsletterSubscribers(NewsletterInfo newsletter)
        {
            var contactsWithEmail = ContactInfo.Provider.Get().WhereNotEmpty(nameof(ContactInfo.ContactEmail));
            var contactSubscriberIds = GetNewsletterContactSubscriberIds(newsletter.NewsletterID);
            var contactGroupMemberIds = GetNewsletterContactGroupMemberIds(newsletter.NewsletterID);

            return contactsWithEmail.Where(where => where
                .WhereIn(nameof(ContactInfo.ContactID), contactSubscriberIds)
                .Or()
                .WhereIn(nameof(ContactInfo.ContactID), contactGroupMemberIds)
            ).TypedResult;
        }


        private ObjectQuery<ContactGroupMemberInfo> GetNewsletterContactGroupMemberIds(int newsletterId)
        {
            var groupSubscriberIds = SubscriberInfo.Provider.Get()
                .Column(nameof(SubscriberInfo.SubscriberRelatedID))
                .WhereEquals(nameof(SubscriberInfo.SubscriberType), PredefinedObjectType.CONTACTGROUP)
                .WhereIn(
                    nameof(SubscriberInfo.SubscriberID),
                    SubscriberNewsletterInfoProvider.GetApprovedSubscriberNewsletters()
                        .Column(nameof(SubscriberInfo.SubscriberID))
                        .WhereEquals(nameof(NewsletterInfo.NewsletterID), newsletterId));

            var contactGroupIds = ContactGroupInfo.Provider.Get()
                .Column(nameof(ContactGroupInfo.ContactGroupID))
                .WhereIn(nameof(ContactGroupInfo.ContactGroupID), groupSubscriberIds);

            return ContactGroupMemberInfo.Provider.Get()
                .Column(nameof(ContactGroupMemberInfo.ContactGroupMemberRelatedID))
                .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberType), (int)ContactGroupMemberTypeEnum.Contact)
                .WhereIn(nameof(ContactGroupMemberInfo.ContactGroupMemberContactGroupID), contactGroupIds);
        }


        private ObjectQuery<SubscriberInfo> GetNewsletterContactSubscriberIds(int newsletterId)
        {
            return SubscriberInfo.Provider.Get()
                .Column(nameof(SubscriberInfo.SubscriberRelatedID))
                .WhereEquals(nameof(SubscriberInfo.SubscriberType), PredefinedObjectType.CONTACT)
                .WhereIn(
                    nameof(SubscriberInfo.SubscriberID),
                    SubscriberNewsletterInfoProvider.GetApprovedSubscriberNewsletters()
                        .Column(nameof(SubscriberInfo.SubscriberID))
                        .WhereEquals(nameof(NewsletterInfo.NewsletterID), newsletterId));
        }
    }
}