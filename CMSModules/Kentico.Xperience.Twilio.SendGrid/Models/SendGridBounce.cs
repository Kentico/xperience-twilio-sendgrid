namespace Kentico.Xperience.Twilio.SendGrid.Models
{
    /// <summary>
    /// Represents an individual bounce record retrieved by the SendGrid API.
    /// </summary>
    /// <remarks>See <see href="https://docs.sendgrid.com/api-reference/bounces-api/retrieve-all-bounces"/>.</remarks>
    public class SendGridBounce
    {
        /// <summary>
        /// The unix timestamp for when the bounce record was created at SendGrid.
        /// </summary>
        public long Created
        {
            get;
            set;
        }


        /// <summary>
        /// The email address that was added to the bounce list.
        /// </summary>
        public string Email
        {
            get;
            set;
        }


        /// <summary>
        /// The reason for the bounce. This typically will be a bounce code, an enhanced code, and a description.
        /// </summary>
        public string Reason
        {
            get;
            set;
        }


        /// <summary>
        /// Enhanced SMTP bounce response.
        /// </summary>
        public string Status
        {
            get;
            set;
        }
    }
}