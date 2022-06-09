namespace Kentico.Xperience.Twilio.SendGrid.Models
{
    /// <summary>
    /// SendGrid integration options.
    /// </summary>
    public sealed class SendGridOptions
    {
        /// <summary>
        /// Configuration section name.
        /// </summary>
        public const string SECTION_NAME = "xperience.twilio.sendgrid";


        /// <summary>
        /// The SendGrid API key.
        /// </summary>
        public string ApiKey
        {
            get;
            set;
        }
    }
}
