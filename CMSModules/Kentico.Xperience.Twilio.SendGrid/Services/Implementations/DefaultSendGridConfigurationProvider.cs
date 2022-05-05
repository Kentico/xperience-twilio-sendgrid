using CMS;
using CMS.Core;
using CMS.Helpers;

using Kentico.Xperience.Twilio.SendGrid.Services;

using SendGrid.Helpers.Mail;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;

[assembly: RegisterImplementation(typeof(ISendGridConfigurationProvider), typeof(DefaultSendGridConfigurationProvider), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.Twilio.SendGrid.Services
{
    /// <summary>
    /// Default implementation of <see cref="ISendGridConfigurationProvider"/> which retrieves the SendGrid configuration
    /// from the application settings.
    /// </summary>
    public class DefaultSendGridConfigurationProvider : ISendGridConfigurationProvider
    {
        public void SetIpPoolName(string siteName, SendGridMessage sendGridMessage)
        {
            var ipPoolName = ValidationHelper.GetString(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_IP_POOL_NAME], String.Empty);
            if (!String.IsNullOrEmpty(ipPoolName))
            {
                sendGridMessage.IpPoolName = ipPoolName;
            }
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
                    Enable = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_SANDBOX_MODE], false)
                }
            };

            var bypassListManagement = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_BYPASS_GROUPS_SUPPRESSION], false);
            var bypassSpam = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_BYPASS_SPAM], false);
            var bypassUnsubscribes = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_BYPASS_GLOBAL_UNSUBSCRIBE], false);
            var bypassBounces = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_BYPASS_BOUNCE], false);
            var enabledMutuallyExclusiveSettings = new List<bool>
            {
                bypassListManagement,
                bypassSpam,
                bypassUnsubscribes,
                bypassBounces
            };
            if (enabledMutuallyExclusiveSettings.Where(setting => setting == true).Count() > 1)
            {
                var message = $"The application settings {SendGridConstants.APPSETTING_BYPASS_BOUNCE}, {SendGridConstants.APPSETTING_BYPASS_GLOBAL_UNSUBSCRIBE}, {SendGridConstants.APPSETTING_BYPASS_GROUPS_SUPPRESSION},"
                    + $" and {SendGridConstants.APPSETTING_BYPASS_SPAM} are mutually exclusive. Please check your web.config and enable only one of these settings at maximum.";
                throw new InvalidOperationException(message);
            }

            if (bypassListManagement)
            {
                mailSettings.BypassListManagement = new BypassListManagement()
                {
                    Enable = true
                };
            }
            if (bypassSpam)
            {
                mailSettings.BypassSpamManagement = new BypassSpamManagement()
                {
                    Enable = true
                };
            }
            if (bypassUnsubscribes)
            {
                mailSettings.BypassUnsubscribeManagement = new BypassUnsubscribeManagement()
                {
                    Enable = true
                };
            }
            if (bypassBounces)
            {
                mailSettings.BypassBounceManagement = new BypassBounceManagement()
                {
                    Enable = true
                };
            }

            sendGridMessage.MailSettings = mailSettings;
        }


        public void SetTrackingSettings(string siteName, SendGridMessage sendGridMessage)
        {
            var trackingSettings = new TrackingSettings()
            {
                ClickTracking = new ClickTracking()
                {
                    Enable = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_ENABLE_CLICK_TRACKING], false)
                },
                OpenTracking = new OpenTracking()
                {
                    Enable = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_ENABLE_OPEN_TRACKING], false)
                },
                SubscriptionTracking = new SubscriptionTracking()
                {
                    Enable = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_ENABLE_SUBSCRIPTION_TRACKING], false)
                },
                Ganalytics = new Ganalytics()
                {
                    Enable = ValidationHelper.GetBoolean(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_ENABLE_GANALYTICS], false)
                }
            };

            sendGridMessage.TrackingSettings = trackingSettings;
        }
    }
}