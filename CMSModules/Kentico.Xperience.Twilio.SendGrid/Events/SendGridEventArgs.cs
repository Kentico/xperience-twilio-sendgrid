using CMS.Base;

using Kentico.Xperience.Twilio.SendGrid.Models;

namespace Kentico.Xperience.Twilio.SendGrid.Events
{
    /// <summary>
    /// Event arguments used in a <see cref="SendGridEventHandler"/>.
    /// </summary>
    public class SendGridEventArgs : CMSEventArgs
    {
        /// <summary>
        /// The SendGrid event which triggered the Xperience event handler.
        /// </summary>
        public SendGridEvent SendGridEvent
        {
            get;
            set;
        }
    }
}