using CMS.Base;
using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

using Kentico.Xperience.Twilio.SendGrid.Models;

using SendGrid;

using System;
using System.Linq;
using System.Net;
using System.Security.Principal;
using System.Text;
using System.Web.UI;

namespace Kentico.Xperience.Twilio.SendGrid.Pages
{
    /// <summary>
    /// A modal dialog which performs a mass action from the SendGrid "Suppressions" UI.
    /// </summary>
    public partial class SuppressionManagementMassAction : CMSAdministrationPage, ICallbackEventHandler
    {
        private const int SHOWN_RECORDS_NUMBER = 500;
        private IEventLogService eventLogService;
        private ISendGridClient sendGridClient;
        private SuppressionManagementActionParameters mParameters;
        private string mParametersKey;


        /// <summary>
        /// All errors that occurred during deletion.
        /// </summary>
        private string CurrentError
        {
            get
            {
                return ctlAsyncLog.ProcessData.Error;
            }
            set
            {
                ctlAsyncLog.ProcessData.Error = value;
            }
        }


        /// <summary>
        /// The parameters of the chosen mass action.
        /// </summary>
        private SuppressionManagementActionParameters Parameters
        {
            get
            {
                return mParameters ?? (mParameters = WindowHelper.GetItem(ParametersKey) as SuppressionManagementActionParameters);
            }
        }


        /// <summary>
        /// The key used to retrieve stored parameters from session.
        /// </summary>
        private string ParametersKey
        {
            get
            {
                return mParametersKey ?? (mParametersKey = QueryHelper.GetString("parameters", String.Empty));
            }
        }


        protected void Page_Init(object sender, EventArgs e)
        {
            eventLogService = Service.Resolve<IEventLogService>();
            sendGridClient = Service.ResolveOptional<ISendGridClient>();

            // Set message placeholder
            if (CurrentMaster != null)
            {
                CurrentMaster.MessagesPlaceHolder = pnlMessagePlaceholder;
            }

            // Register save handler and closing JavaScript 
            var master = CurrentMaster as ICMSModalMasterPage;
            if (master != null)
            {
                master.ShowSaveAndCloseButton();
                master.SetSaveResourceString("Run");
                master.Save += btnRun_OnClick;
                master.SetCloseJavaScript("ReloadAndCallback();");
            }
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            if (RequestHelper.IsCallback() || !ValidateParameters())
            {
                return;
            }

            ScriptHelper.RegisterWOpenerScript(Page);
            RegisterCallbackScript();

            SetAsyncLogParameters();

            TogglePanels(showContent: true);
            LoadContactView();
        }


        protected void btnRun_OnClick(object sender, EventArgs e)
        {
            TogglePanels(showContent: false);
            ctlAsyncLog.EnsureLog();
            ctlAsyncLog.RunAsync(RunProcess, WindowsIdentity.GetCurrent());
        }


        protected void btnCancel_OnClick(object sender, EventArgs e)
        {
            ReturnToListing();
        }


        private void OnFinished(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CurrentError))
            {
                ShowError("Errors occurred during processing.", description: CurrentError);
                LoadContactView();
            }
            else
            {
                ReturnToListing();
            }
        }


        private void OnCancel(object sender, EventArgs e)
        {
            string cancelled = "The process was cancelled.";
            ctlAsyncLog.AddLog(cancelled);
            LoadContactView();
            ShowWarning(cancelled);
        }


        /// <summary>
        /// Sets visibility of content panel and log panel. Only one can be shown at the time.
        /// </summary>
        private void TogglePanels(bool showContent)
        {
            pnlContent.Visible = showContent;
            pnlLog.Visible = !showContent;
        }


        /// <summary>
        /// Registers a callback script that clears session when dialog is closed.
        /// </summary>
        private void RegisterCallbackScript()
        {
            var callbackEventReference = Page.ClientScript.GetCallbackEventReference(this, String.Empty, "CloseDialog", String.Empty);
            var closeJavaScript = $"function ReloadAndCallback() {{ wopener.{Parameters.ReloadScript}; {callbackEventReference} }}";
            ScriptHelper.RegisterClientScriptBlock(Page, GetType(), "ReloadAndCallback", closeJavaScript, true);
        }


        /// <summary>
        /// Sets parameters of <see cref="AsyncControl"/> dialog.
        /// </summary>
        private void SetAsyncLogParameters()
        {
            ctlAsyncLog.TitleText = Parameters.Title;
            ctlAsyncLog.OnFinished += OnFinished;
            ctlAsyncLog.OnCancel += OnCancel;
        }


        /// <summary>
        /// Ensures that all parameters required for the chosen action are valid, and validates the hash.
        /// </summary>
        private bool ValidateParameters()
        {
            QueryHelper.ValidateHash("hash", settings: new HashSettings(String.Empty) { Redirect = true });

            if (Parameters == null)
            {
                HandleInvalidParameters("There were no parameters found under " + ParametersKey + " key.");
                return false;
            }

            if (Parameters.ContactIDs == null ||
                Parameters.ContactIDs.Count == 0 ||
                String.IsNullOrEmpty(Parameters.ReloadScript) ||
                String.IsNullOrEmpty(Parameters.Title))
            {
                HandleInvalidParameters("One or more parameters are invalid:" + Environment.NewLine + Parameters);
                return false;
            }

            if (Parameters.ActionName == SendGridConstants.ACTION_DELETE_SENDGRID_BOUNCE && sendGridClient == null)
            {
                HandleInvalidParameters("The SendGrid client is not configured properly.");
                return false;
            }

            return true;
        }


        /// <summary>
        /// Shows and logs error.
        /// </summary>
        private void HandleInvalidParameters(string eventDescription)
        {
            eventLogService.LogError(nameof(SuppressionManagementMassAction), nameof(HandleInvalidParameters), eventDescription);
            RedirectToInformation("The requirements for the chosen action are invalid, please check the Event Log.");
        }


        /// <summary>
        /// Shows contact email address which will have their bounces deleted.
        /// </summary>
        private void LoadContactView()
        {
            PageTitle.TitleText = Parameters.Title;
            headAnnouncement.Text = "The action will be performed for the following contacts:";
            lblItems.Text = GetDisplayableEmails();
        }


        /// <summary>
        /// Builds HTML string with email addresses of chosen contacts.
        /// </summary>
        private string GetDisplayableEmails()
        {
            var builder = new StringBuilder();
            AppendLimitMessage(builder);

            var emails = GetSelectedContacts()
                .Take(SHOWN_RECORDS_NUMBER)
                .ToList()
                .Where(contact => contact.CheckPermissions(PermissionsEnum.Read, CurrentSiteName, CurrentUser))
                .Select(contact => contact.ContactEmail);
            foreach (var email in emails)
            {
                builder.Append($"<div>&nbsp;{HTMLHelper.HTMLEncode(email)}</div>{Environment.NewLine}");
            }

            // If message is not empty set panel visible
            if (builder.Length > 1)
            {
                pnlItemList.Visible = true;
            }

            return builder.ToString();
        }


        /// <summary>
        /// Eventually appends a message which is shown when more than <see cref="SHOWN_RECORDS_NUMBER"/> contacts are about to be processed.
        /// </summary>
        private void AppendLimitMessage(StringBuilder builder)
        {
            if (Parameters.ContactIDs.Count <= SHOWN_RECORDS_NUMBER)
            {
                return;
            }

            var moreThanMax = String.Format(@"
                <div>
                    <b>{0}</b>
                </div>
                <br />",
                GetString("massdelete.showlimit"));

            builder.AppendFormat(moreThanMax, SHOWN_RECORDS_NUMBER, Parameters.ContactIDs.Count);
        }


        /// <summary>
        /// Performs the chosen mass action within an asynchronous dialog.
        /// </summary>
        private void RunProcess(object parameter)
        {
            var errorLog = new StringBuilder();
            using (var logProgress = new LogContext())
            {
                GetSelectedContacts()
                    .ForEachObject(contact =>
                    {
                        switch (Parameters.ActionName)
                        {
                            case SendGridConstants.ACTION_DELETE_SENDGRID_BOUNCE:
                                DeleteSendGridBounces(contact, errorLog, logProgress);
                                break;
                            case SendGridConstants.ACTION_DELETE_XPERIENCE_BOUNCE:
                                DeleteXperienceBounces(contact, errorLog, logProgress);
                                break;
                        }
                    });
            }

            if (errorLog.Length != 0)
            {
                CurrentError = errorLog.ToString();
            }
        }


        /// <summary>
        /// Sets <see cref="ContactInfo.ContactBounces"/> to zero for the provided <paramref name="contact"/>.
        /// </summary>
        /// <param name="contact">Contact whose bounces will be deleted.</param>
        /// <param name="errorLog"> Log where errors will be recorded.</param>
        /// <param name="logProgress">Log where progress will be recorded.</param>
        private void DeleteXperienceBounces(ContactInfo contact, StringBuilder errorLog, LogContext logProgress)
        {
            var displayableName = HTMLHelper.HTMLEncode(contact.ContactEmail);
            using (new CMSActionContext { LogEvents = false })
            {
                contact.ContactBounces = 0;
                contact.Update();
                AddSuccessLog(logProgress, displayableName);
            }
        }


        /// <summary>
        /// Deletes SendGrid bounces for a single contact. Handles logging to event log and to deleting page.
        /// </summary>
        /// <param name="contact">Contact whose bounces will be deleted.</param>
        /// <param name="errorLog"> Log where errors will be recorded.</param>
        /// <param name="logProgress">Log where progress will be recorded.</param>
        private void DeleteSendGridBounces(ContactInfo contact, StringBuilder errorLog, LogContext logProgress)
        {
            var displayableName = HTMLHelper.HTMLEncode(contact.ContactEmail);
            using (new CMSActionContext { LogEvents = false })
            {
                var queryParams = $"{{ 'email_address': '{contact.ContactEmail}' }}";
                var response = sendGridClient.RequestAsync(
                    method: BaseClient.Method.DELETE,
                    urlPath: $"suppression/bounces/{contact.ContactEmail}",
                    queryParams: queryParams
                ).ConfigureAwait(false).GetAwaiter().GetResult();
                if (response.IsSuccessStatusCode)
                {
                    AddSuccessLog(logProgress, displayableName);
                    return;
                }

                if (response.StatusCode == HttpStatusCode.NotFound)
                {
                    // Not found is not considered an error
                    AddSuccessLog(logProgress, displayableName);
                    return;
                }

                AddErrorLog(errorLog, $"Couldn't delete bounces for {contact.ContactEmail}.");
            }
        }


        /// <summary>
        /// Logs successful delete.
        /// </summary>
        /// <param name="logProgress">Log where successful delete will be recorded.</param>
        /// <param name="displayableName">Name of successfully deleted item.</param>
        private void AddSuccessLog(LogContext logProgress, string displayableName)
        {
            ctlAsyncLog.AddLog(displayableName);
            string deletedMessage = $"Bounces for {displayableName} were deleted.";
            logProgress.LogEvent(EventType.INFORMATION, nameof(SuppressionManagementMassAction), "DeleteBounces", deletedMessage, RequestContext.RawURL, CurrentUser.UserID, CurrentUser.UserName,
                0, null, RequestContext.UserHostAddress, SiteContext.CurrentSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
        }


        /// <summary>
        /// Appends the error information to <paramref name="errorLog"/>.
        /// </summary>
        /// <param name="errorLog">Log where errors will be recorded.</param>
        /// <param name="message">Message to add to the log.</param>
        private void AddErrorLog(StringBuilder errorLog, string message)
        {
            ctlAsyncLog.AddLog(message);
            errorLog.Append($"<div>{message}</div>{Environment.NewLine}");
        }


        /// <summary>
        /// Gets the contacts to run the process against.
        /// </summary>
        private ObjectQuery<ContactInfo> GetSelectedContacts()
        {
            return ContactInfo.Provider.Get()
                .WhereIn(nameof(ContactInfo.ContactID), Parameters.ContactIDs);
        }


        /// <summary>
        /// Redirects back to parent listing.
        /// </summary>
        private void ReturnToListing()
        {
            WindowHelper.Remove(ParametersKey);

            var script = @"wopener." + Parameters.ReloadScript + "; CloseDialog();";
            ScriptHelper.RegisterStartupScript(Page, GetType(), "ReloadGridAndClose", script, addScriptTags: true);
        }


        public void RaiseCallbackEvent(string eventArgument)
        {
            // Raised when Close button in the dialog is clicked, so the parameters can be cleared from session
            WindowHelper.Remove(ParametersKey);
        }


        public string GetCallbackResult()
        {
            // CloseDialog JavaScript method is called to receive the callback results, thus no data needs to be passed to it
            return String.Empty;
        }
    }
}