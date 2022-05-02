using CMS.Base;
using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using CMS.SiteProvider;
using CMS.UIControls;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web.UI;

namespace Kentico.Xperience.Twilio.SendGrid.Pages
{
    public partial class DeleteXperienceBounce : CMSAdministrationPage, ICallbackEventHandler
    {
        private string mParametersKey;
        private const int SHOWN_RECORDS_NUMBER = 500;
        private IEventLogService eventLogService;


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
        /// Identifiers of the contacts whose bounces will be deleted.
        /// </summary>
        private ICollection<int> ContactIds
        {
            get;
            set;
        }


        /// <summary>
        /// Key used to retrieve stored parameters from session.
        /// </summary>
        private string ParametersKey
        {
            get
            {
                return mParametersKey ?? (mParametersKey = QueryHelper.GetString("parameters", String.Empty));
            }
        }


        /// <summary>
        /// JavaScript for parent page reload when delete is shown in modal dialog.
        /// </summary>
        private string ReloadScript
        {
            get;
            set;
        }


        protected void Page_Init(object sender, EventArgs e)
        {
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
                master.SetSaveResourceString("general.delete");
                master.Save += btnDelete_OnClick;
                master.SetCloseJavaScript("ReloadAndCallback();");
            }

            eventLogService = Service.Resolve<IEventLogService>();
        }


        protected void Page_Load(object sender, EventArgs e)
        {
            QueryHelper.ValidateHash("hash", settings: new HashSettings(String.Empty) { Redirect = true });

            if (RequestHelper.IsCallback() || !InitializeProperties())
            {
                return;
            }

            ScriptHelper.RegisterWOpenerScript(Page);
            RegisterCallbackScript();

            SetAsyncLogParameters();

            TogglePanels(showContent: true);
            LoadContactView();
        }


        protected void btnDelete_OnClick(object sender, EventArgs e)
        {
            TogglePanels(showContent: false);
            ctlAsyncLog.EnsureLog();
            ctlAsyncLog.RunAsync(DeleteAllBounces, WindowsIdentity.GetCurrent());
        }


        protected void btnCancel_OnClick(object sender, EventArgs e)
        {
            ReturnToListing();
        }


        private void OnFinished(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(CurrentError))
            {
                ShowError("Errors occurred during bounce deletion.", description: CurrentError);
                LoadContactView();
            }
            else
            {
                ReturnToListing();
            }
        }


        private void OnCancel(object sender, EventArgs e)
        {
            string cancelled = "Bounce deletion was cancelled.";
            ctlAsyncLog.AddLog(cancelled);
            LoadContactView();
            ShowWarning(cancelled);
        }


        /// <summary>
        /// Sets visibility of content panel and log panel.
        /// Only one can be shown at the time.
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
            var closeJavaScript = $"function ReloadAndCallback() {{ wopener.{ReloadScript}; {callbackEventReference} }}";
            ScriptHelper.RegisterClientScriptBlock(Page, GetType(), "ReloadAndCallback", closeJavaScript, true);
        }


        /// <summary>
        /// Sets parameters of <see cref="AsyncControl"/> dialog.
        /// </summary>
        private void SetAsyncLogParameters()
        {
            ctlAsyncLog.TitleText = "Deleting Xperience bounces";
            ctlAsyncLog.OnFinished += OnFinished;
            ctlAsyncLog.OnCancel += OnCancel;
        }


        /// <summary>
        /// Retrieves parameters from the session.
        /// Returns <c>true</c> when all properties were set successfully.
        /// </summary>
        private bool InitializeProperties()
        {
            var parameters = WindowHelper.GetItem(ParametersKey) as MassActionParameters;
            if (parameters == null)
            {
                HandleInvalidParameters("There were no parameters found under " + ParametersKey + " key.");
                return false;
            }

            ContactIds = (parameters.IDs ?? Enumerable.Empty<int>()).ToList();
            ReloadScript = parameters.ReloadScript;

            if (ContactIds == null || ContactIds.Count == 0 || String.IsNullOrEmpty(ReloadScript))
            {
                HandleInvalidParameters("One or more parameters are invalid:" + Environment.NewLine + parameters);
                return false;
            }

            return true;
        }


        /// <summary>
        /// Shows and logs error.
        /// </summary>
        private void HandleInvalidParameters(string eventDescription)
        {
            eventLogService.LogError(nameof(DeleteXperienceBounce), nameof(HandleInvalidParameters), eventDescription);
            RedirectToInformation(GetString("massdelete.invalidparameters"));
        }


        /// <summary>
        /// Shows contact email address which will have their bounces deleted.
        /// </summary>
        private void LoadContactView()
        {
            PageTitle.TitleText = "Deleting Xperience bounces";
            headAnnouncement.Text = "The following contacts will have their bounces deleted:";
            lblItems.Text = GetDisplayableEmails();
        }


        /// <summary>
        /// Builds HTML string with email addresses of chosen contacts.
        /// </summary>
        private string GetDisplayableEmails()
        {
            var builder = new StringBuilder();
            AppendLimitMessage(builder);

            var emails = GetContactsToDeleteBounces()
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
            if (ContactIds.Count <= SHOWN_RECORDS_NUMBER)
            {
                return;
            }

            var moreThanMax = String.Format(@"
                <div>
                    <b>{0}</b>
                </div>
                <br />",
                GetString("massdelete.showlimit"));

            builder.AppendFormat(moreThanMax, SHOWN_RECORDS_NUMBER, ContactIds.Count);
        }


        /// <summary>
        /// Handles deleting of contact bounces within asynchronous dialog.
        /// </summary>
        private void DeleteAllBounces(object parameter)
        {
            var errorLog = new StringBuilder();
            using (var logProgress = new LogContext())
            {
                GetContactsToDeleteBounces()
                    .ForEachObject(contact =>
                    {
                        var displayableName = HTMLHelper.HTMLEncode(contact.ContactEmail);
                        using (new CMSActionContext { LogEvents = false })
                        {
                            contact.ContactBounces = 0;
                            contact.Update();
                            AddSuccessLog(logProgress, displayableName);
                        }
                    });
            }

            if (errorLog.Length != 0)
            {
                CurrentError = errorLog.ToString();
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
            logProgress.LogEvent(EventType.INFORMATION, nameof(DeleteXperienceBounce), "DeleteBounces", deletedMessage, RequestContext.RawURL, CurrentUser.UserID, CurrentUser.UserName, 0, null, RequestContext.UserHostAddress, SiteContext.CurrentSiteID, SystemContext.MachineName, RequestContext.URLReferrer, RequestContext.UserAgent, DateTime.Now);
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
        /// Gets contacts to clear the bounces for.
        /// </summary>
        private ObjectQuery<ContactInfo> GetContactsToDeleteBounces()
        {
            return ContactInfo.Provider.Get()
                .WhereIn(nameof(ContactInfo.ContactID), ContactIds);
        }


        /// <summary>
        /// Redirects back to parent listing.
        /// </summary>
        private void ReturnToListing()
        {
            WindowHelper.Remove(ParametersKey);

            var script = @"wopener." + ReloadScript + "; CloseDialog();";
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