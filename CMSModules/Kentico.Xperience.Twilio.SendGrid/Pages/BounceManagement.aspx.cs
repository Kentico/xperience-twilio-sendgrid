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
    [UIElement("Kentico.Xperience.Twilio.SendGrid", "BounceManagement")]
    [EditedObject(IDQueryParameter = "objectid", ObjectType = NewsletterInfo.OBJECT_TYPE)]
    public partial class BounceManagement : CMSPage
    {
        private IEnumerable<SendGridBounce> bounceData;
        

        protected void Page_Load(object sender, EventArgs e)
        {
            LoadBounceData();
            LoadGridData();
            
            gridReport.OnExternalDataBound += gridReport_OnExternalDataBound;
        }


        private object gridReport_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            if (bounceData == null)
            {
                return parameter;
            }

            switch (sourceName)
            {
                case "sg-bounced":
                    var email = ValidationHelper.GetString(parameter, String.Empty);
                    return UniGridFunctions.ColoredSpanYesNo(bounceData.Any(b => b.Email == email));
                case "kx-bounced":
                    // TODO: Check if bounce monitoring is enabled?
                    var drv = parameter as DataRowView;
                    var settingsService = Service.Resolve<ISettingsService>();
                    var contactBounces = ValidationHelper.GetInteger(drv[nameof(ContactInfo.ContactBounces)], 0);
                    var bounceLimit = ValidationHelper.GetInteger(settingsService["CMSBouncedEmailsLimit"], 0);
                    return UniGridFunctions.ColoredSpanYesNo(contactBounces >= bounceLimit);
            }

            return parameter;
        }


        private ObjectQuery<ContactGroupMemberInfo> GetContactGroupMemberIds(int newsletterId)
        {
            var groupSubscriberIds = SubscriberInfo.Provider.Get()
                .Column(nameof(SubscriberInfo.SubscriberRelatedID))
                .WhereEquals(nameof(SubscriberInfo.SubscriberType), PredefinedObjectType.CONTACTGROUP)
                .WhereIn(
                    nameof(SubscriberInfo.SubscriberID),
                    SubscriberNewsletterInfoProvider.GetApprovedSubscriberNewsletters()
                        .Column(nameof(SubscriberInfo.SubscriberID))
                        .WhereEquals(nameof(NewsletterInfo.NewsletterID), newsletterId));

            var contactGroupsIds = ContactGroupInfo.Provider.Get()
                .Column(nameof(ContactGroupInfo.ContactGroupID))
                .WhereIn(nameof(ContactGroupInfo.ContactGroupID), groupSubscriberIds);

            return ContactGroupMemberInfo.Provider.Get()
                .Column(nameof(ContactGroupMemberInfo.ContactGroupMemberRelatedID))
                .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberType), (int)ContactGroupMemberTypeEnum.Contact)
                .WhereIn(nameof(ContactGroupMemberInfo.ContactGroupMemberContactGroupID), contactGroupsIds);
        }


        private ObjectQuery<SubscriberInfo> GetContactSubscriberIds(int newsletterId)
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


        private void LoadBounceData()
        {
            var sendGridClient = Service.Resolve<ISendGridClient>();
            var response = sendGridClient.RequestAsync(
                method: BaseClient.Method.GET,
                urlPath: "suppression/bounces"
            ).ConfigureAwait(false).GetAwaiter().GetResult();
            var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                bounceData = JsonConvert.DeserializeObject<IEnumerable<SendGridBounce>>(responseBody);
            }
            else
            {
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
                eventLogService.LogError(nameof(BounceManagement), nameof(Page_Load), logDescription);
                ShowError("Unable to load bounces from SendGrid. Please check the Event Log.");
            }
        }


        private void LoadGridData()
        {
            if (gridReport.StopProcessing)
            {
                return;
            }

            var newsletterId = (EditedObject as NewsletterInfo).NewsletterID;
            var contactsWithEmail = ContactInfo.Provider.Get().WhereNotEmpty(nameof(ContactInfo.ContactEmail));
            var contactSubscriberIds = GetContactSubscriberIds(newsletterId);
            var contactGroupMemberIds = GetContactGroupMemberIds(newsletterId);

            var subscribedContacts = contactsWithEmail.Where(where => where
                .WhereIn(nameof(ContactInfo.ContactID), contactSubscriberIds)
                .Or()
                .WhereIn(nameof(ContactInfo.ContactID), contactGroupMemberIds)
            ).TypedResult;

            gridReport.DataSource = subscribedContacts;
            gridReport.DataBind();
        }
    }
}