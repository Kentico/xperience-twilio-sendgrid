using CMS.Base;

using Kentico.Xperience.Twilio.SendGrid.Models;

namespace Kentico.Xperience.Twilio.SendGrid.Events
{
    /// <summary>
    /// An Xperience event handler which is triggered when a SendGrid event is received via webhook.
    /// </summary>
    public class SendGridEventHandler : AdvancedHandler<SendGridEventHandler, SendGridEventArgs>
    {
        /// <summary>
        /// Initiates the event handling.
        /// </summary>
        /// <param name="eventObject">The SendGrid event which triggered the Xperience event handler.</param>
        public void StartEvent(SendGridEvent eventObject)
        {
            var e = new SendGridEventArgs
            {
                SendGridEvent = eventObject
            };
            
            using (var h = StartEvent(e))
            {
                if (h.Continue)
                {
                    h.Finish();
                }
            }
        }
    }
}