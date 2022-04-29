using CMS.ContactManagement;
using CMS.Core;
using CMS.Helpers;
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
    public partial class BounceManagement : CMSPage
    {
        private IEnumerable<SendGridBounce> bounces;
        private IEventLogService eventLogService;
        private ISendGridClient sendGridClient;
        private ISettingsService settingsService;
        

        protected void Page_Load(object sender, EventArgs e)
        {
            eventLogService = Service.Resolve<IEventLogService>();
            sendGridClient = Service.Resolve<ISendGridClient>();
            settingsService = Service.Resolve<ISettingsService>();

            var response = sendGridClient.RequestAsync(
                method: BaseClient.Method.GET,
                urlPath: "suppression/bounces"
            ).ConfigureAwait(false).GetAwaiter().GetResult();
            var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                bounces = JsonConvert.DeserializeObject<IEnumerable<SendGridBounce>>(responseBody);
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
                eventLogService.LogError(nameof(BounceManagement), nameof(Page_Load), logDescription);
                ShowError("Unable to load bounces from SendGrid. Please check the Event Log.");
            }

            gridReport.OnAction += gridReport_OnAction;
            gridReport.OnExternalDataBound += gridReport_OnExternalDataBound;
        }


        private void gridReport_OnAction(string actionName, object actionArgument)
        {
            switch (actionName)
            {
                case "purge-sg":
                    PurgeSendGridBounce(actionArgument);
                    break;
                case "purge-kx":
                    PurgeXperienceBounces(actionArgument);
                    break;
            }
        }


        private object gridReport_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            if (bounces == null)
            {
                return parameter;
            }

            switch (sourceName)
            {
                case "sg-bounced":
                    var email = ValidationHelper.GetString(parameter, String.Empty);
                    return UniGridFunctions.ColoredSpanYesNo(bounces.Any(b => b.Email == email));
                case "kx-bounced":
                    // TODO: Check if bounce monitoring is enabled?
                    var drv = parameter as DataRowView;
                    var contactBounces = ValidationHelper.GetInteger(drv[nameof(ContactInfo.ContactBounces)], 0);
                    var bounceLimit = ValidationHelper.GetInteger(settingsService["CMSBouncedEmailsLimit"], 0);
                    return UniGridFunctions.ColoredSpanYesNo(contactBounces >= bounceLimit);
            }

            return parameter;
        }


        private void PurgeSendGridBounce(object actionArgument)
        {
            var email = ValidationHelper.GetString(actionArgument, String.Empty);
            if (String.IsNullOrEmpty(email))
            {
                ShowError("Unable to load contact email.");
            }
            if (!bounces.Any(b => b.Email == email))
            {
                ShowInformation("The email address is not on the SendGrid bounce list.");
                return;
            }

            var queryParams = $"{{ 'email_address': '{email}' }}";
            var response = sendGridClient.RequestAsync(
                method: BaseClient.Method.DELETE,
                urlPath: $"suppression/bounces/{email}",
                queryParams: queryParams
            ).ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                URLHelper.Redirect(UIContext.UIElement.ElementTargetURL);
            }
            else
            {
                var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var responseError = JsonConvert.DeserializeObject<SendGridApiErrorResponse>(responseBody);
                var errorDescriptions = responseError.Errors.Select(err =>
                {
                    var prefix = String.IsNullOrEmpty(err.Field) ? String.Empty : $"Field \"{err.Field}\": ";
                    return $"- {prefix}\"{err.Message}\"";
                });
                var logDescription = $"Unable delete bounce from SendGrid:\r\n\r\n{String.Join("\r\n", errorDescriptions)}";
                eventLogService.LogError(nameof(BounceManagement), nameof(Page_Load), logDescription);
                ShowError("Unable to delete bounce from SendGrid. Please check the Event Log.");
            }
        }


        private void PurgeXperienceBounces(object actionArgument)
        {
            var contactId = ValidationHelper.GetInteger(actionArgument, 0);
            if (contactId == 0)
            {
                ShowError("Unable to load contact identifier.");
            }

            var contact = ContactInfo.Provider.Get(contactId);
            if (contact == null)
            {
                ShowError("Unable to load contact information.");
            }

            if (contact.ContactBounces == 0)
            {
                ShowInformation("The contact has no bounces in Xperience.");
                return;
            }

            contact.ContactBounces = 0;
            contact.Update();
        }
    }
}