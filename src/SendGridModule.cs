using CMS;
using CMS.Base;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;

using Kentico.Xperience.Twilio.SendGrid;

using SendGrid;

using System;
using System.Configuration;

[assembly: AssemblyDiscoverable]
[assembly: RegisterModule(typeof(SendGridModule))]
namespace Kentico.Xperience.Twilio.SendGrid
{
    /// <summary>
    /// A custom module which registers services for the CMS application.
    /// </summary>
    public class SendGridModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridModule"/> class.
        /// </summary>
        public SendGridModule() : base(nameof(SendGridModule))
        {
        }


        protected override void OnPreInit()
        {
            base.OnPreInit();

            if (SystemContext.IsCMSRunningAsMainApplication)
            {
                var apiKey = ValidationHelper.GetString(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_API_KEY], String.Empty);
                if (!String.IsNullOrEmpty(apiKey))
                {
                    Service.Use<ISendGridClient>(new SendGridClient(apiKey));
                }
            }
        }
    }
}