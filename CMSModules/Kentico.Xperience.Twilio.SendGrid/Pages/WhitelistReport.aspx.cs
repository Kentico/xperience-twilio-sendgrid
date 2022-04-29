using CMS.Base.Web.UI;
using CMS.ContactManagement;
using CMS.Core;
using CMS.Helpers;
using CMS.UIControls;

using Kentico.Xperience.Twilio.SendGrid.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using SendGrid;

using System;
using System.Data;
using System.Linq;
using System.Web.UI.WebControls;

namespace Kentico.Xperience.SendGrid.Pages
{
    /// <summary>
    /// An Xperience module interface page which allows for management of the SendGrid whitelist setting.
    /// </summary>
    [UIElement("Kentico.Xperience.Twilio.SendGrid", "WhitelistReport")]
    public partial class WhitelistReport : CMSPage
    {
        private ISendGridClient sendGridClient;
        private WhitelistSettingsResponse whitelistSettings;


        protected void Page_Load(object sender, EventArgs e)
        {
            sendGridClient = Service.Resolve<ISendGridClient>();
            var response = sendGridClient.RequestAsync(
                method: BaseClient.Method.GET,
                urlPath: "mail_settings/address_whitelist"
            ).ConfigureAwait(false).GetAwaiter().GetResult();
            var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                whitelistSettings = JsonConvert.DeserializeObject<WhitelistSettingsResponse>(responseBody);
                if (whitelistSettings.Enabled)
                {
                    btnDisableWhitelist.Visible = true;
                }
                else
                {
                    gridReport.Visible = false;
                    btnEnableWhitelist.Visible = true;
                    ShowInformation("SendGrid whitelist is disabled.");
                }
            }
            else
            {
                // TODO: Log error
                var responseError = JsonConvert.DeserializeObject<SendGridApiErrorResponse>(responseBody);
            }

            gridReport.OnAction += gridReport_OnAction;
            gridReport.OnExternalDataBound += gridReport_OnExternalDataBound;
        }


        /// <summary>
        /// Click handler which enables the SendGrid whitelist.
        /// </summary>
        protected void btnEnableWhitelist_Click(object sender, EventArgs e)
        {
            ToggleWhitelistSetting(true);
        }


        /// <summary>
        /// Click handler which disables the SendGrid whitelist.
        /// </summary>
        protected void btnDisableWhitelist_Click(object sender, EventArgs e)
        {
            ToggleWhitelistSetting(false);
        }


        private void gridReport_OnAction(string actionName, object actionArgument)
        {
            switch (actionName)
            {
                case "addemail":
                    var emailToAdd = ValidationHelper.GetString(actionArgument, String.Empty);
                    UpdateWhitelist(emailToAdd, true);
                    break;
                case "adddomain":
                    var emailDomainToAdd = ValidationHelper.GetString(actionArgument, String.Empty);
                    if (String.IsNullOrEmpty(emailDomainToAdd))
                    {
                        // TODO: Log error
                    }

                    UpdateWhitelist(emailDomainToAdd.Split('@')[1], true);
                    break;
                case "removeemail":
                    var emailToRemove = ValidationHelper.GetString(actionArgument, String.Empty);
                    UpdateWhitelist(emailToRemove, false);
                    break;
                case "removedomain":
                    var emailDomainToRemove = ValidationHelper.GetString(actionArgument, String.Empty);
                    if (String.IsNullOrEmpty(emailDomainToRemove))
                    {
                        // TODO: Log error
                    }

                    UpdateWhitelist(emailDomainToRemove.Split('@')[1], false);
                    break;
            }
        }


        private object gridReport_OnExternalDataBound(object sender, string sourceName, object parameter)
        {
            if (whitelistSettings == null)
            {
                return parameter;
            }

            switch (sourceName)
            {
                case "whitelisted":
                    var email = ValidationHelper.GetString(parameter, String.Empty);
                    return UniGridFunctions.ColoredSpanYesNo(IsEmailWhitelisted(email));
                case "addemail":
                    var drv = (parameter as GridViewRow).DataItem as DataRowView;
                    var emailToAdd = ValidationHelper.GetString(drv[nameof(ContactInfo.ContactEmail)], String.Empty);
                    var addEmailButton = (CMSGridActionButton)sender;
                    if (IsEmailWhitelisted(emailToAdd))
                    {
                        addEmailButton.Visible = false;
                        break;
                    }
                    if (whitelistSettings.List.Contains(emailToAdd))
                    {
                        addEmailButton.Enabled = false;
                        addEmailButton.ToolTip = "Email is already included in whitelist.";
                    }
                    break;
                case "adddomain":
                    var drv2 = (parameter as GridViewRow).DataItem as DataRowView;
                    var emailDomainToAdd = ValidationHelper.GetString(drv2[nameof(ContactInfo.ContactEmail)], String.Empty);
                    var addDomainButton = (CMSGridActionButton)sender;
                    if (IsEmailWhitelisted(emailDomainToAdd))
                    {
                        addDomainButton.Visible = false;
                        break;
                    }
                    if (whitelistSettings.List.Contains(emailDomainToAdd.Split('@')[1]))
                    {
                        addDomainButton.Enabled = false;
                        addDomainButton.ToolTip = "Domain is already included in whitelist.";
                    }
                    break;
                case "removeemail":
                    var drv3 = (parameter as GridViewRow).DataItem as DataRowView;
                    var emailToRemove = ValidationHelper.GetString(drv3[nameof(ContactInfo.ContactEmail)], String.Empty);
                    var removeEmailButton = (CMSGridActionButton)sender;
                    if (!IsEmailWhitelisted(emailToRemove))
                    {
                        removeEmailButton.Visible = false;
                        break;
                    }
                    
                    if (!whitelistSettings.List.Contains(emailToRemove))
                    {
                        removeEmailButton.Enabled = false;
                        removeEmailButton.ToolTip = "Whitelist doesn't contain this email address.";
                    }
                    break;
                case "removedomain":
                    var drv4 = (parameter as GridViewRow).DataItem as DataRowView;
                    var emailDomainToRemove = ValidationHelper.GetString(drv4[nameof(ContactInfo.ContactEmail)], String.Empty);
                    var removeDomainButton = (CMSGridActionButton)sender;
                    if (!IsEmailWhitelisted(emailDomainToRemove))
                    {
                        removeDomainButton.Visible = false;
                        break;
                    }

                    if (!whitelistSettings.List.Contains(emailDomainToRemove.Split('@')[1]))
                    {
                        removeDomainButton.Enabled = false;
                        removeDomainButton.ToolTip = "Whitelist doesn't contain this domain.";
                    }
                    break;
            }

            return parameter;
        }


        private void ToggleWhitelistSetting(bool enable)
        {
            var data = new JObject(new JProperty("enabled", enable));
            var response = sendGridClient.RequestAsync(
                method: SendGridClient.Method.PATCH,
                urlPath: "mail_settings/address_whitelist",
                requestBody: data.ToString()
            ).ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                URLHelper.Redirect(UIContext.UIElement.ElementTargetURL);
            }
            else
            {
                // TODO: Log error
                var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var responseError = JsonConvert.DeserializeObject<SendGridApiErrorResponse>(responseBody);
            }
        }


        private void UpdateWhitelist(string item, bool add)
        {
            var whitelist = whitelistSettings.List.ToList();
            if (String.IsNullOrEmpty(item))
            {
                return;
            }

            if (add)
            {
                whitelist.Add(item);
            }
            else
            {
                whitelist.Remove(item);
            }

            var data = new JObject(new JProperty("list", whitelist));
            var response = sendGridClient.RequestAsync(
                method: SendGridClient.Method.PATCH,
                urlPath: "mail_settings/address_whitelist",
                requestBody: data.ToString()
            ).ConfigureAwait(false).GetAwaiter().GetResult();
            if (response.IsSuccessStatusCode)
            {
                URLHelper.Redirect(UIContext.UIElement.ElementTargetURL);
            }
            else
            {
                // TODO: Log error
                var responseBody = response.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                var responseError = JsonConvert.DeserializeObject<SendGridApiErrorResponse>(responseBody);
            }
        }


        private bool IsEmailWhitelisted(string email)
        {
            if (String.IsNullOrEmpty(email) || whitelistSettings.List == null)
            {
                return false;
            }

            var emailDomain = email.Split('@')[1];
            return whitelistSettings.List.Contains(email) || whitelistSettings.List.Contains(emailDomain);
        }
    }
}