using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;
using CMS.Newsletters.Web.UI;
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
    /// <summary>
    /// An administration UI page which displays subscribers of a newsletter or email campaign issue and allows
    /// mass actions to be performed on the subscribers.
    /// </summary>
    public partial class SuppressionManagement : CMSNewsletterPage
    {
        private IEnumerable<SendGridBounce> bounceData;
        private int? mBounceLimit;
        private BaseInfo mEditedNewsletterOrIssue;


        /// <summary>
        /// The email marketing bounce limit value.
        /// </summary>
        private int? BounceLimit
        {
            get
            {
                if (mBounceLimit == null)
                {
                    mBounceLimit = NewsletterHelper.BouncedEmailsLimit(CurrentSiteName);
                }

                return mBounceLimit;
            }
        }


        /// <summary>
        /// The newsletter or email campaign issue being displayed in the UI.
        /// </summary>
        private BaseInfo EditedNewsletterOrIssue
        {
            get
            {
                if (mEditedNewsletterOrIssue == null)
                {
                    var objectId = QueryHelper.GetInteger("objectid", 0);
                    var parentId = QueryHelper.GetInteger("parentobjectid", 0);
                    if (parentId > 0)
                    {
                        // We are editing an issue of a campaign
                        mEditedNewsletterOrIssue = IssueInfo.Provider.Get(objectId);
                    }
                    else
                    {
                        // We are editing a newsletter
                        mEditedNewsletterOrIssue = NewsletterInfo.Provider.Get(objectId);
                    }
                }

                return mEditedNewsletterOrIssue;
            }
        }


        /// <summary>
        /// The function that is used to generate the <see cref="MassActionItem.CreateUrl"/> delegate for mass actions.
        /// </summary>
        private Func<Func<SuppressionManagementActionParameters, string>, string, string, CreateUrlDelegate> FunctionConverter
        {
            get
            {
                return (generateActionFunction, actionName, title) =>
                {
                    return (scope, selectedNodeIDs, parameters) =>
                    {
                        List<int> contactIds = new List<int>();
                        switch (scope)
                        {
                            case MassActionScopeEnum.AllItems:
                                contactIds.AddRange(GetSubscribers(gridReport.GetFilter()).Select(c => c.ContactID).ToList());
                                break;
                            case MassActionScopeEnum.SelectedItems:
                                contactIds.AddRange(selectedNodeIDs);
                                break;
                        }

                        var suppressionManagementParameters = new SuppressionManagementActionParameters
                        {
                            ActionName = actionName,
                            Title = title,
                            ContactIDs = contactIds,
                            ReloadScript = gridReport.GetReloadScript()
                        };
                        return generateActionFunction(suppressionManagementParameters);
                    };
                };
            }
        }


        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            ScriptHelper.RegisterDialogScript(Page);

            bounceData = GetBounceData();
            if (bounceData == null)
            {
                gridReport.StopProcessing = true;
                ctrlMassActions.Visible = false;
                ctrlMassActions.StopProcessing = true;
                return;
            }

            LoadGridMassActions();
            gridReport.OnDataReload += gridReport_OnDataReload;
            gridReport.OnExternalDataBound += gridReport_OnExternalDataBound;
        }


        /// <summary>
        /// Returns the subscribers of the current <see cref="EditedNewsletterOrIssue"/>.
        /// </summary>
        private DataSet gridReport_OnDataReload(string completeWhere, string currentOrder, int currentTopN, string columns, int currentOffset, int currentPageSize, ref int totalRecords)
        {
            if (gridReport.StopProcessing)
            {
                return new DataSet();
            }

            var query = GetSubscribers(gridReport.GetFilter());
            if (query == null)
            {
                return new DataSet();
            }

            query = query.Where(completeWhere)
                .OrderBy(currentOrder)
                .TopN(currentTopN)
                .Columns(columns);

            query.MaxRecords = currentPageSize;
            query.Offset = currentOffset;

            totalRecords = query.TotalRecords;

            return query.Result;
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
                    var contactBounces = ValidationHelper.GetInteger(parameter, 0);
                    var cssClass = contactBounces >= BounceLimit ? "StatusDisabled" : "StatusEnabled";
                    return $"<span class='{cssClass}'>{contactBounces}</span>";
            }

            return parameter;
        }


        /// <summary>
        /// Returns a cached list of emails on the SendGrid bounce suppression list or null if there are errors.
        /// </summary>
        private IEnumerable<SendGridBounce> GetBounceData()
        {
            return CacheHelper.Cache((cs) => {
                var sendGridClient = Service.ResolveOptional<ISendGridClient>();
                if (sendGridClient == null)
                {
                    ShowError("The SendGrid client is not configured properly.");
                    cs.Cached = false;
                    return null;
                }

                var response = sendGridClient.RequestAsync(
                    method: BaseClient.Method.GET,
                    urlPath: "suppression/bounces"
                ).ConfigureAwait(false).GetAwaiter().GetResult();
                var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<IEnumerable<SendGridBounce>>(responseBody);
                }

                var responseError = JsonConvert.DeserializeObject<SendGridApiErrorResponse>(responseBody);
                var errorDescriptions = responseError.Errors.Select(err =>
                {
                    var prefix = String.IsNullOrEmpty(err.Field) ? String.Empty : $"Field \"{err.Field}\": ";
                    return $"- {prefix}\"{err.Message}\"";
                });
                var logDescription = $"Unable to load bounces from SendGrid:\r\n\r\n{String.Join("\r\n", errorDescriptions)}";
                var eventLogService = Service.Resolve<IEventLogService>();

                eventLogService.LogError(nameof(SuppressionManagement), nameof(GetBounceData), logDescription);
                ShowError("Unable to load bounces from SendGrid. Please check the Event Log.");
                return null;

            }, new CacheSettings(20, SendGridConstants.CACHE_KEY_BOUNCES));
        }


        /// <summary>
        /// Sets the actions of the <see cref="ctrlMassActions"/> control and configures the delegate which stores the
        /// <see cref="SuppressionManagementActionParameters"/> in session and generates the proper URL to the modal window.
        /// </summary>
        private void LoadGridMassActions()
        {
            if (gridReport.StopProcessing)
            {
                return;
            }

            ctrlMassActions.SelectedItemsClientID = gridReport.GetSelectionFieldClientID();
            ctrlMassActions.SelectedItemsResourceString = "Selected subscribers";
            ctrlMassActions.AllItemsResourceString = "All subscribers";
            ctrlMassActions.AddMassActions(
                new MassActionItem
                {
                    ActionType = MassActionTypeEnum.OpenModal,
                    CodeName = "Reset Xperience Bounces",
                    CreateUrl = FunctionConverter(GetMassActionUrl, SendGridConstants.ACTION_DELETE_XPERIENCE_BOUNCE, "Reset Xperience Bounces")
                },
                new MassActionItem
                {
                    ActionType = MassActionTypeEnum.OpenModal,
                    CodeName = "Reset SendGrid Bounces",
                    CreateUrl = FunctionConverter(GetMassActionUrl, SendGridConstants.ACTION_DELETE_SENDGRID_BOUNCE, "Reset SendGrid Bounces")
                }
            );
        }


        /// <summary>
        /// Stores the <paramref name="suppressionManagementParameters"/> in session and generates the absolute URL
        /// to the mass action modal window.
        /// </summary>
        /// <param name="suppressionManagementParameters">Parameters related to the chosen mass action and selected contacts.</param>
        private string GetMassActionUrl(SuppressionManagementActionParameters suppressionManagementParameters)
        {
            var paramsIdentifier = Guid.NewGuid().ToString();
            WindowHelper.Add(paramsIdentifier, suppressionManagementParameters);

            var url = URLHelper.ResolveUrl(SendGridConstants.URL_SUPPRESSION_MASSACTION);
            url = URLHelper.AddParameterToUrl(url, "parameters", paramsIdentifier);
            url = URLHelper.AddParameterToUrl(url, "hash", QueryHelper.GetHash(URLHelper.GetQuery(url)));

            return url;
        }


        /// <summary>
        /// Gets the subscribers of the current newsletter or email campaign issue.
        /// </summary>
        /// <param name="additionalWhere">A where condition to apply to the subscriber query.</param>
        private ObjectQuery<ContactInfo> GetSubscribers(string additionalWhere = null)
        {
            ObjectQuery<ContactInfo> query = new ObjectQuery<ContactInfo>();
            if (EditedNewsletterOrIssue is NewsletterInfo)
            {
                query = LoadNewsletterSubscribers(EditedNewsletterOrIssue as NewsletterInfo);
            }
            else if (EditedNewsletterOrIssue is IssueInfo)
            {
                query = LoadCampaignIssueSubscribers(EditedNewsletterOrIssue as IssueInfo);
            }

            return query.Where(additionalWhere);
        }


        private ObjectQuery<ContactInfo> LoadCampaignIssueSubscribers(IssueInfo issue)
        {
            var contactsWithEmail = ContactInfo.Provider.Get()
                .WhereNotEmpty(nameof(ContactInfo.ContactEmail));
            var contactGroupIds = IssueContactGroupInfo.Provider.Get()
                .Column(nameof(IssueContactGroupInfo.ContactGroupID))
                .WhereEquals(nameof(IssueContactGroupInfo.IssueID), issue.IssueID);
            var contactGroupMemberIds = ContactGroupMemberInfo.Provider.Get()
                .Column(nameof(ContactGroupMemberInfo.ContactGroupMemberRelatedID))
                .WhereEquals(nameof(ContactGroupMemberInfo.ContactGroupMemberType), (int)ContactGroupMemberTypeEnum.Contact)
                .WhereIn(nameof(ContactGroupMemberInfo.ContactGroupMemberContactGroupID), contactGroupIds);
            return contactsWithEmail.Where(where => where
                .WhereIn(nameof(ContactInfo.ContactID), contactGroupMemberIds)
            );
        }


        private ObjectQuery<ContactInfo> LoadNewsletterSubscribers(NewsletterInfo newsletter)
        {
            var contactsWithEmail = ContactInfo.Provider.Get()
                .WhereNotEmpty(nameof(ContactInfo.ContactEmail));
            var contactSubscriberIds = GetNewsletterContactSubscriberIds(newsletter.NewsletterID);
            var contactGroupMemberIds = GetNewsletterContactGroupMemberIds(newsletter.NewsletterID);

            return contactsWithEmail.Where(where => where
                .WhereIn(nameof(ContactInfo.ContactID), contactSubscriberIds)
                .Or()
                .WhereIn(nameof(ContactInfo.ContactID), contactGroupMemberIds)
            );
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