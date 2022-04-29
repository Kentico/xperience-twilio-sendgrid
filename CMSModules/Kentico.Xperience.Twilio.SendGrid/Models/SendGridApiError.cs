namespace Kentico.Xperience.Twilio.SendGrid.Models
{
    /// <summary>
    /// Represents an individual error which can occur in a <see cref="SendGridApiErrorResponse"/>.
    /// </summary>
    public class SendGridApiError
    {
        /// <summary>
        /// The error ID.
        /// </summary>
        public string ErrorId
        {
            get;
            set;
        }


        /// <summary>
        /// The field in the request body which caused the error, if any.
        /// </summary>
        public string Field
        {
            get;
            set;
        }


        /// <summary>
        /// A help link to resolve the error.
        /// </summary>
        public string Help
        {
            get;
            set;
        }


        /// <summary>
        /// The error details.
        /// </summary>
        public string Message
        {
            get;
            set;
        }
    }
}