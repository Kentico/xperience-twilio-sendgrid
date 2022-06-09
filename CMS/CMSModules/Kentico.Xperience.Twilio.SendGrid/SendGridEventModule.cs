using CMS;
using CMS.DataEngine;

using Kentico.Xperience.Twilio.SendGrid;

using System.Web.Http;

[assembly: RegisterModule(typeof(SendGridEventModule))]
namespace Kentico.Xperience.Twilio.SendGrid
{
    /// <summary>
    /// A custom module which registers .NET Web API controller routes to handle SendGrid webhooks.
    /// </summary>
    public class SendGridEventModule : Module
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridEventModule"/> class.
        /// </summary>
        public SendGridEventModule() : base(nameof(SendGridEventModule))
        {
        }


        protected override void OnPreInit()
        {
            base.OnPreInit();

            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "xperience-sendgridevents",
                "xperience-sendgrid/events",
                defaults: new { controller = "SendGrid", action = "ReceiveEvents" }
            );
        }
    }
}