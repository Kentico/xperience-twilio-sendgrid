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
        /// The name of the <see cref="SendGridEventHandler"/> that was triggered which corresponds
        /// with a value from <see cref="SendGridEventName"/>.
        /// </summary>
        public string EventName
        {
            get;
            set;
        }


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