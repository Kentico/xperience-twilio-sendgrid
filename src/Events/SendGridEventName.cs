using Kentico.Xperience.Twilio.SendGrid.Models;

namespace Kentico.Xperience.Twilio.SendGrid.Events
{
    /// <summary>
    /// Contains constants for all possible values of <see cref="SendGridEvent.Event"/> or <see cref="SendGridEvent.Type"/>.
    /// </summary>
    public static class SendGridEventName
    {
        /// <summary>
        /// Triggers when a receiving server could not or would not accept the message temporarily.
        /// </summary>
        public const string Blocked = "blocked";


        /// <summary>
        /// Triggers when a message bounces, i.e. the receiving server will not accept mail for a recipient permanently.
        /// </summary>
        public const string Bounce = "bounce";


        /// <summary>
        /// Triggers when a recipient clicks on a link within the message.
        /// </summary>
        public const string Click = "click";


        /// <summary>
        /// Triggers when a receiving server temporarily rejects a message.
        /// </summary>
        public const string Deferred = "deferred";


        /// <summary>
        /// Triggers when a message has been successfully delivered to the receiving server.
        /// </summary>
        public const string Delivered = "delivered";


        /// <summary>
        /// Triggers when a message is dropped, i.e. purposefully not delivered.
        /// </summary>
        public const string Dropped = "dropped";


        /// <summary>
        /// Triggers when a recipient unsubscribes from a specific group either by clicking the link directly or updating their preferences.
        /// </summary>
        public const string GroupUnsubscribe = "group_unsubscribe";


        /// <summary>
        /// Triggers when a recipient resubscribes to a specific group by updating their preferences.
        /// </summary>
        public const string GroupResubscribe = "group_resubscribe";


        /// <summary>
        /// Triggers when a recipient has opened the HTML message.
        /// </summary>
        public const string Open = "open";


        /// <summary>
        /// Triggers when a message has been received and is ready to be delivered.
        /// </summary>
        public const string Processed = "processed";


        /// <summary>
        /// Triggers when a recipient marks a message as spam.
        /// </summary>
        public const string SpamReport = "spamreport";


        /// <summary>
        /// Triggers when a recipient clicks on the 'Opt Out of All Emails' link.
        /// </summary>
        public const string Unsubscribe = "unsubscribe";
    }
}