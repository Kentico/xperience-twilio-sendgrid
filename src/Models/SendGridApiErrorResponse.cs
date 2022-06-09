using System.Collections.Generic;

namespace Kentico.Xperience.Twilio.SendGrid.Models
{
    /// <summary>
    /// Represents the response of an erroneous SendGrid web API request.
    /// </summary>
    public class SendGridApiErrorResponse
    {
        /// <summary>
        /// One or more errors caused by the web API request.
        /// </summary>
        public IEnumerable<SendGridApiError> Errors
        {
            get;
            set;
        }
    }
}