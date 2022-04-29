using SendGrid.Helpers.Mail;

namespace Kentico.Xperience.Twilio.SendGrid.Services
{
    /// <summary>
    /// Contains methods for retrieving the local configuration of the SendGrid integration.
    /// </summary>
    public interface ISendGridConfigurationProvider
    {
        /// <summary>
        /// Gets the IP pool name to send emails from.
        /// </summary>
        /// <param name="siteName">The Xperience site name.</param>
        string GetIpPoolName(string siteName);


        /// <summary>
        /// Gets the SendGrid <see cref="MailSettings"/> to apply to an email.
        /// </summary>
        /// <param name="siteName">The Xperience site name.</param>
        MailSettings GetMailSettings(string siteName);


        /// <summary>
        /// Gets the SendGrid <see cref="TrackingSettings"/> to apply to an email.
        /// </summary>
        /// <param name="siteName">The Xperience site name.</param>
        TrackingSettings GetTrackingSettings(string siteName);
    }
}