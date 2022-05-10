using CMS;
using CMS.ContactManagement;
using CMS.Core;
using CMS.DataEngine;
using CMS.Helpers;
using CMS.Newsletters;

using Kentico.Xperience.Twilio.SendGrid;
using Kentico.Xperience.Twilio.SendGrid.Events;

using SendGrid;

using System;
using System.Configuration;
using System.Linq;
using System.Web.Http;

[assembly: RegisterModule(typeof(SendGridModule))]
namespace Kentico.Xperience.Twilio.SendGrid
{
    /// <summary>
    /// A custom module which initializes the SendGrid integration's Dependency Injection, .NET Web API Controllers,
    /// and Xperience event handlers.
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

            // Map controller routes
            GlobalConfiguration.Configuration.Routes.MapHttpRoute(
                "xperience-sendgridevents",
                "xperience-sendgrid/events",
                defaults: new { controller = "SendGrid", action = "ReceiveEvents" }
            );

            var apiKey = ValidationHelper.GetString(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_API_KEY], String.Empty);
            if (!String.IsNullOrEmpty(apiKey))
            {
                Service.Use<ISendGridClient>(new SendGridClient(apiKey));
            }

            // Register event handlers
            SendGridEvents.Bounce.After += MarkIssueUndelivered;
            SendGridEvents.Drop.After += MarkIssueUndelivered;
            SendGridEvents.Block.After += MarkIssueUndelivered;
        }


        /// <summary>
        /// Increment a newsletter issue's <see cref="IssueInfo.IssueBounces"/> when an email can't be delivered.
        /// Also increments a contact's <see cref="ContactInfo.ContactBounces"/> after receiving a SendGrid "bounce"
        /// event webhook.
        /// </summary>
        private void MarkIssueUndelivered(object sender, SendGridEventArgs e)
        {
            var issueId = ValidationHelper.GetInteger(e.SendGridEvent.IssueId, 0);
            var issueInfo = IssueInfo.Provider.Get(issueId);
            if (issueInfo != null)
            {
                issueInfo.IssueBounces++;
                issueInfo.Update();
            }

            if (e.SendGridEvent.Event == "bounce")
            {
                var contact = ContactInfo.Provider.Get()
                .TopN(1)
                .WhereEquals(nameof(ContactInfo.ContactEmail), e.SendGridEvent.Email)
                .FirstOrDefault();
                if (contact == null)
                {
                    return;
                }

                contact.ContactBounces++;
                contact.Update();
            }
        }
    }
}