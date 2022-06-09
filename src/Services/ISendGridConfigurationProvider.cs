using SendGrid.Helpers.Mail;

namespace Kentico.Xperience.Twilio.SendGrid.Services
{
    /// <summary>
    /// Contains methods for getting or setting the local configuration of the SendGrid integration.
    /// </summary>
    public interface ISendGridConfigurationProvider
    {
        /// <summary>
        /// Sets the IP pool name of the <paramref name="sendGridMessage"/>.
        /// </summary>
        /// <param name="siteName">The Xperience site name.</param>
        /// <param name="sendGridMessage">The SendGrid email being processed.</param>
        void SetIpPoolName(string siteName, SendGridMessage sendGridMessage);


        /// <summary>
        /// Sets the SendGrid <see cref="MailSettings"/> to the <paramref name="sendGridMessage"/>.
        /// </summary>
        /// <param name="siteName">The Xperience site name.</param>
        /// <param name="sendGridMessage">The SendGrid email being processed.</param>
        void SetMailSettings(string siteName, SendGridMessage sendGridMessage);


        /// <summary>
        /// Sets the SendGrid <see cref="TrackingSettings"/> to the <paramref name="sendGridMessage"/>.
        /// </summary>
        /// <param name="siteName">The Xperience site name.</param>
        /// <param name="sendGridMessage">The SendGrid email being processed.</param>
        void SetTrackingSettings(string siteName, SendGridMessage sendGridMessage);
    }
}