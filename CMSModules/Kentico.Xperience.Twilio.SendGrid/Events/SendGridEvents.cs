namespace Kentico.Xperience.Twilio.SendGrid.Events
{
    /// <summary>
    /// Xperience events which correspond with SendGrid events.
    /// </summary>
    /// <remarks>See <see href="https://docs.sendgrid.com/for-developers/tracking-events/event"/>.</remarks>
    public static class SendGridEvents
    {
        /// <summary>
        /// Triggers when a receiving server could not or would not accept the message temporarily.
        /// </summary>
        public static readonly SendGridEventHandler Block = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a message bounces, i.e. the receiving server will not accept mail for a recipient permanently.
        /// </summary>
        public static readonly SendGridEventHandler Bounce = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a recipient clicks on a link within the message.
        /// </summary>
        public static readonly SendGridEventHandler Click = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a receiving server temporarily rejects a message.
        /// </summary>
        public static readonly SendGridEventHandler Defer = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a message has been successfully delivered to the receiving server.
        /// </summary>
        public static readonly SendGridEventHandler Deliver = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a message is dropped, i.e. purposefully not delivered.
        /// </summary>
        public static readonly SendGridEventHandler Drop = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a recipient unsubscribes from a specific group either by clicking the link directly or updating their preferences.
        /// </summary>
        public static readonly SendGridEventHandler GroupUnsubscribe = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a recipient resubscribes to a specific group by updating their preferences.
        /// </summary>
        public static readonly SendGridEventHandler GroupResubscribe = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a recipient has opened the HTML message.
        /// </summary>
        public static readonly SendGridEventHandler Open = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a message has been received and is ready to be delivered.
        /// </summary>
        public static readonly SendGridEventHandler Process = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a recipient marks a message as spam.
        /// </summary>
        public static readonly SendGridEventHandler SpamReport = new SendGridEventHandler();


        /// <summary>
        /// Triggers when a recipient clicks on the 'Opt Out of All Emails' link.
        /// </summary>
        public static readonly SendGridEventHandler Unsubscribe = new SendGridEventHandler();
    }
}