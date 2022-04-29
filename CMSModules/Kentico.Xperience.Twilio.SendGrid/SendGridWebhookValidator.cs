using CMS.Core;
using CMS.Helpers;

using EllipticCurve;

using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net.Http;

namespace Kentico.Xperience.Twilio.SendGrid
{
    /// <summary>
    /// Validates the headers of a SendGrid webhook to verify the sender of a request.
    /// </summary>
    public class SendGridWebhookValidator
    {
        private string payload;
        private HttpRequestMessage request;


        /// <summary>
        /// Initializes a new instance of the <see cref="SendGridWebhookValidator"/> class.
        /// </summary>
        /// <param name="payload">The full body of the SendGrid webhook request.</param>
        /// <param name="request">The current request.</param>
        public SendGridWebhookValidator(string payload, HttpRequestMessage request)
        {
            this.payload = payload;
            this.request = request;
        }


        /// <summary>
        /// Returns true if the request originated from SendGrid.
        /// </summary>
        public bool VerifySignature()
        {
            var eventLogService = Service.Resolve<IEventLogService>();
            if (String.IsNullOrEmpty(payload) || request == null)
            {
                eventLogService.LogError(nameof(SendGridWebhookValidator), nameof(VerifySignature), "SendGridWebhookValidator parameters are invalid.");
                return false;
            }

            var webhookKey = ValidationHelper.GetString(ConfigurationManager.AppSettings[SendGridConstants.APPSETTING_WEBHOOK_KEY], String.Empty);
            if (String.IsNullOrEmpty(webhookKey))
            {
                eventLogService.LogError(nameof(SendGridWebhookValidator), nameof(VerifySignature), $"Unable to load the {SendGridConstants.APPSETTING_WEBHOOK_KEY} application setting.");
                return false;
            }

            var publicKey = PublicKey.fromPem(webhookKey);
            IEnumerable<string> signatureValues, timestampValues;
            if (!request.Headers.TryGetValues(SendGridConstants.HEADER_WEBHOOK_SIGNATURE, out signatureValues) ||
                !request.Headers.TryGetValues(SendGridConstants.HEADER_WEBHOOK_TIMESTAMP, out timestampValues))
            {
                eventLogService.LogError(nameof(SendGridWebhookValidator), nameof(VerifySignature), "Unable to load the SendGrid request headers.");
                return false;
            }

            var signature = signatureValues.FirstOrDefault();
            var timestamp = timestampValues.FirstOrDefault();
            if (String.IsNullOrEmpty(signature) || String.IsNullOrEmpty(timestamp))
            {
                eventLogService.LogError(nameof(SendGridWebhookValidator), nameof(VerifySignature), "The SendGrid request headers are invalid.");
                return false;
            }

            var timestampedPayload = timestamp + payload;
            var decodedSignature = Signature.fromBase64(signature);
            var isValid = Ecdsa.verify(timestampedPayload, decodedSignature, publicKey);

            eventLogService.LogError(nameof(SendGridWebhookValidator), nameof(VerifySignature), "Received an invalid request signature.");
            return isValid;
        }
    }
}