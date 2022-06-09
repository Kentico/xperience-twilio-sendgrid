using Kentico.Xperience.Twilio.SendGrid.Events;
using Kentico.Xperience.Twilio.SendGrid.Models;

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
                case SendGridEventName.Processed:
                    SendGridEvents.Processed.StartEvent(eventObject);
                    break;
                case SendGridEventName.Dropped:
                    SendGridEvents.Dropped.StartEvent(eventObject);
                    break;
                case SendGridEventName.Delivered:
                    SendGridEvents.Delivered.StartEvent(eventObject);
                    break;
                case SendGridEventName.Deferred:
                    SendGridEvents.Deferred.StartEvent(eventObject);
                    break;
                case SendGridEventName.Bounce:
                    SendGridEvents.Bounce.StartEvent(eventObject);
                    break;
                case SendGridEventName.Blocked:
                    SendGridEvents.Blocked.StartEvent(eventObject);
                    break;
                case SendGridEventName.Open:
                    SendGridEvents.Open.StartEvent(eventObject);
                    break;
                case SendGridEventName.Click:
                    SendGridEvents.Click.StartEvent(eventObject);
                    break;
                case SendGridEventName.SpamReport:
                    SendGridEvents.SpamReport.StartEvent(eventObject);
                    break;
                case SendGridEventName.Unsubscribe:
                    SendGridEvents.Unsubscribe.StartEvent(eventObject);
                    break;
                case SendGridEventName.GroupUnsubscribe:
                    SendGridEvents.GroupUnsubscribe.StartEvent(eventObject);
                    break;
                case SendGridEventName.GroupResubscribe:
                    SendGridEvents.GroupResubscribe.StartEvent(eventObject);
                    break;
            }
        }
    }
}