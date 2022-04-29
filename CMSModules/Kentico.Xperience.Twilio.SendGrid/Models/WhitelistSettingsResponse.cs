using System.Collections.Generic;

namespace Kentico.Xperience.Twilio.SendGrid.Models
{
    /// <summary>
    /// The response of a web API request to retrieve the SendGrid whitelist settings.
    /// </summary>
    /// <remarks>See <see href="https://docs.sendgrid.com/api-reference/settings-mail/retrieve-address-whitelist-mail-settings"/>.</remarks>
    public class WhitelistSettingsResponse
    {
        /// <summary>
        /// Indicates whether the whitelist setting is enabled.
        /// </summary>
        public bool Enabled
        {
            get;
            set;
        }


        /// <summary>
        /// A list of email addresses or domains that are whitelisted.
        /// </summary>
        public IEnumerable<string> List
        {
            get;
            set;
        }
    }
}