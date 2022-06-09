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
        }


        public void SetMailSettings(string siteName, SendGridMessage sendGridMessage)
        {
            var mailSettings = new MailSettings()
            {
                FooterSettings = new FooterSettings()
                {
                    Enable = false
                },
                SpamCheck = new SpamCheck()
                {
                    Enable = false
                },
                BccSettings = new BCCSettings()
                {
                    Enable = false
                },
                SandboxMode = new SandboxMode()
                {
                    Enable = false
                }
                /* Only one of these features may enabled at a time.
                BypassListManagement = new BypassListManagement()
                {
                    Enable = false
                },
                BypassSpamManagement = new BypassSpamManagement()
                {
                    Enable = false
                },
                BypassUnsubscribeManagement = new BypassUnsubscribeManagement()
                {
                    Enable = true
                },
                BypassBounceManagement = new BypassBounceManagement()
                {
                    Enable = true
                }
                */
            };

            sendGridMessage.MailSettings = mailSettings;
        }


        public void SetTrackingSettings(string siteName, SendGridMessage sendGridMessage)
        {
            var trackingSettings = new TrackingSettings()
            {
                ClickTracking = new ClickTracking()
                {
                    Enable = false
                },
                OpenTracking = new OpenTracking()
                {
                    Enable = false
                },
                SubscriptionTracking = new SubscriptionTracking()
                {
                    Enable = false
                },
                Ganalytics = new Ganalytics()
                {
                    Enable = false
                }
            };

            sendGridMessage.TrackingSettings = trackingSettings;
        }
    }
}