using CMS;
using CMS.Core;

using Kentico.Xperience.Twilio.SendGrid.Services;

using SendGrid.Helpers.Mail;

[assembly: RegisterImplementation(typeof(ISendGridConfigurationProvider), typeof(DefaultSendGridConfigurationProvider), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.Twilio.SendGrid.Services
{
    /// <summary>
    /// Default implementation of <see cref="ISendGridConfigurationProvider"/> which doesn't set any custom mail
    /// or tracking settings.
    /// </summary>
    public class DefaultSendGridConfigurationProvider : ISendGridConfigurationProvider
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSendGridConfigurationProvider"/> class.
        /// </summary>
        public DefaultSendGridConfigurationProvider()
        {
        }


        public void SetIpPoolName(string siteName, SendGridMessage sendGridMessage)
        {
            // Don't set IP Pool name by default
        }


        public void SetMailSettings(string siteName, SendGridMessage sendGridMessage)
        {
            sendGridMessage.MailSettings = new MailSettings();
        }


        public void SetTrackingSettings(string siteName, SendGridMessage sendGridMessage)
        {
            sendGridMessage.TrackingSettings = new TrackingSettings();
        }
    }
}