using CMS.Core;

using Kentico.Xperience.Twilio.SendGrid.Events;
using Kentico.Xperience.Twilio.SendGrid.Models;
using Kentico.Xperience.Twilio.SendGrid.Services;

using Newtonsoft.Json;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace Kentico.Xperience.Twilio.SendGrid.Controllers
{
    /// <summary>
    /// A .NET Web API Controller used in receiving event webhooks from SendGrid.
    /// </summary>
    public class SendGridController : ApiController
    {
        /// <summary>
        /// The endpoint which receives an array of events from SendGrid and calls the appropriate Xperience
        /// event handler for processing, found in the <see cref="SendGridEvents"/> class.
        /// </summary>
        [HttpPost]
        public async Task<HttpResponseMessage> ReceiveEvents()
        {
            var content = await Request.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (String.IsNullOrEmpty(content))
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

            if (Service.Resolve<ISendGridConfigurationProvider>().DebugEnabled())
            {
                Service.Resolve<IEventLogService>().LogInformation(nameof(SendGridController), nameof(ReceiveEvents), $"Received event webhook:\r\n\r\n{content}");
            }

            var validator = new SendGridWebhookValidator(content, Request);
            if (!validator.VerifySignature())
            {
                return Request.CreateResponse(HttpStatusCode.Unauthorized);
            }

            var eventObjects = JsonConvert.DeserializeObject<IEnumerable<SendGridEvent>>(content);
            foreach (var eventObject in eventObjects)
            {
                HandleEvent(eventObject);
            }

            return Request.CreateResponse(HttpStatusCode.OK);
        }


        private void HandleEvent(SendGridEvent eventObject)
        {
            if (String.IsNullOrEmpty(eventObject.Event) || String.IsNullOrEmpty(eventObject.Email))
            {
                return;
            }

            // Prefer type over event for bounce and block differentiation
            var eventName = eventObject.Event;
            if (!String.IsNullOrEmpty(eventObject.Type))
            {
                eventName = eventObject.Type;
            }

            switch (eventName)
            {
                case "processed":
                    SendGridEvents.Process.StartEvent(eventObject);
                    break;
                case "dropped":
                    SendGridEvents.Drop.StartEvent(eventObject);
                    break;
                case "delivered":
                    SendGridEvents.Deliver.StartEvent(eventObject);
                    break;
                case "deferred":
                    SendGridEvents.Defer.StartEvent(eventObject);
                    break;
                case "bounce":
                    SendGridEvents.Bounce.StartEvent(eventObject);
                    break;
                case "blocked":
                    SendGridEvents.Block.StartEvent(eventObject);
                    break;
                case "open":
                    SendGridEvents.Open.StartEvent(eventObject);
                    break;
                case "click":
                    SendGridEvents.Click.StartEvent(eventObject);
                    break;
                case "spamreport":
                    SendGridEvents.SpamReport.StartEvent(eventObject);
                    break;
                case "unsubscribe":
                    SendGridEvents.Unsubscribe.StartEvent(eventObject);
                    break;
                case "group_unsubscribe":
                    SendGridEvents.GroupUnsubscribe.StartEvent(eventObject);
                    break;
                case "group_resubscribe":
                    SendGridEvents.GroupResubscribe.StartEvent(eventObject);
                    break;
            }
        }
    }
}