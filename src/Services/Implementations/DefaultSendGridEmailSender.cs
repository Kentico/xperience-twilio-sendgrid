using CMS;
using CMS.Core;
using CMS.EmailEngine;

using Kentico.Xperience.Twilio.SendGrid.Models;
using Kentico.Xperience.Twilio.SendGrid.Services;

using Newtonsoft.Json;

using SendGrid;
using SendGrid.Helpers.Mail;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Net.Mime;

[assembly: RegisterImplementation(typeof(ISendGridEmailSender), typeof(DefaultSendGridEmailSender), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.SystemDefault)]
namespace Kentico.Xperience.Twilio.SendGrid.Services
{
    /// <summary>
    /// Default implementation of <see cref="ISendGridEmailSender"/>.
    /// </summary>
    public class DefaultSendGridEmailSender : ISendGridEmailSender
    {
        private readonly IEventLogService eventLogService;
        private readonly ISendGridClient sendGridClient;
        private readonly ISendGridConfigurationProvider sendGridConfigurationProvider;


        /// <summary>
        /// Initializes a new instance of the <see cref="DefaultSendGridEmailSender"/> class.
        /// </summary>
        public DefaultSendGridEmailSender(
            IEventLogService eventLogService,
            ISendGridClient sendGridClient,
            ISendGridConfigurationProvider sendGridConfigurationProvider)
        {
            this.eventLogService = eventLogService;
            this.sendGridClient = sendGridClient;
            this.sendGridConfigurationProvider = sendGridConfigurationProvider;
        }


        public SendGridMessage ConvertMailMessage(MailMessage mailMessage, string siteName)
        {
            try
            {
                var sendGridMessage = new SendGridMessage
                {
                    Subject = mailMessage.Subject,
                    From = new EmailAddress(mailMessage.From.Address, mailMessage.From.DisplayName)
                };
                if (mailMessage.To.Count > 0)
                {
                    sendGridMessage.AddTos(mailMessage.To.Select(to => new EmailAddress(to.Address)).ToList());
                }
                if (mailMessage.CC.Count > 0)
                {
                    sendGridMessage.AddCcs(mailMessage.CC.Select(cc => new EmailAddress(cc.Address)).ToList());
                }
                if (mailMessage.Bcc.Count > 0)
                {
                    sendGridMessage.AddBccs(mailMessage.Bcc.Select(bcc => new EmailAddress(bcc.Address)).ToList());
                }

                // Add newsletter header values for retrieval in events
                var xperienceHeaders = mailMessage.Headers.AllKeys.ToDictionary(k => k, k => mailMessage.Headers[k].Trim());
                if (xperienceHeaders.Count > 0)
                {
                    sendGridMessage.AddGlobalCustomArgs(xperienceHeaders);
                }

                AddMessageContents(mailMessage, sendGridMessage);
                AddMessageAttachments(mailMessage, sendGridMessage);

                // Configure settings
                sendGridConfigurationProvider.SetMailSettings(siteName, sendGridMessage);
                sendGridConfigurationProvider.SetTrackingSettings(siteName, sendGridMessage);
                sendGridConfigurationProvider.SetIpPoolName(siteName, sendGridMessage);

                return sendGridMessage;
            }
            catch (Exception ex)
            {
                eventLogService.LogError(nameof(DefaultSendGridEmailSender), nameof(ConvertMailMessage), ex.Message);
            }

            return null;
        }


        public AsyncCompletedEventArgs SendEmail(MailMessage message, string siteName, EmailToken emailToken = null)
        {
            Exception foundException = null;
            var sendGridMessage = ConvertMailMessage(message, siteName);
            if (!IsEmailValid(sendGridMessage))
            {
                foundException = new Exception("One or more email properties were not valid. Please check the Event Log for more details.");
                return new AsyncCompletedEventArgs(foundException, true, emailToken);
            }

            try
            {
                var result = sendGridClient.SendEmailAsync(sendGridMessage).ConfigureAwait(false).GetAwaiter().GetResult();
                var responseBody = result.Body.ReadAsStringAsync().ConfigureAwait(false).GetAwaiter().GetResult();
                if (!result.IsSuccessStatusCode)
                {
                    var responseError = JsonConvert.DeserializeObject<SendGridApiErrorResponse>(responseBody);
                    var errorDescriptions = responseError.Errors.Select(err =>
                    {
                        var prefix = String.IsNullOrEmpty(err.Field) ? String.Empty : $"Field \"{err.Field}\": ";
                        return $"- {prefix}\"{err.Message}\"";
                    });
                    var logDescription = $"Unable to dispatch email \"{sendGridMessage.Subject}\" to SendGrid:\r\n\r\n{String.Join("\r\n", errorDescriptions)}";

                    foundException = new Exception(logDescription);
                    eventLogService.LogError(nameof(DefaultSendGridEmailSender), nameof(SendEmail), logDescription);
                }
            }
            catch (Exception ex)
            {
                foundException = ex;
                eventLogService.LogError(nameof(DefaultSendGridEmailSender), nameof(SendEmail), $"Unable to dispatch email \"{sendGridMessage.Subject}\" to SendGrid:\r\n\"{ex.Message}\"");
            }

            return new AsyncCompletedEventArgs(foundException, foundException != null, emailToken);
        }


        /// <summary>
        /// Adds attachments from the Xperience email to the SendGrid email.
        /// </summary>
        /// <param name="mailMessage">The Xperience email to retreive content from.</param>
        /// <param name="sendGridMessage">The SendGrid email to set the content of.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException"></exception>
        private void AddMessageAttachments(MailMessage mailMessage, SendGridMessage sendGridMessage)
        {
            foreach (var attachment in mailMessage.Attachments)
            {
                var disposition = attachment.ContentDisposition.Inline && mailMessage.IsBodyHtml ? "inline" : "attachment";
                var data = ConvertAttachmentToBase64(attachment.ContentStream);
                if (!String.IsNullOrEmpty(data))
                {
                    sendGridMessage.AddAttachment(attachment.Name, data, attachment.ContentType.MediaType, disposition, attachment.ContentId);
                }
            }
        }


        /// <summary>
        /// Populates the <see cref="SendGridMessage.Contents"/> from the provided <paramref name="mailMessage"/>.
        /// </summary>
        /// <param name="mailMessage">The Xperience email to retreive content from.</param>
        /// <param name="sendGridMessage">The SendGrid email to set the content of.</param>
        /// <exception cref="IOException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="FormatException"></exception>
        private void AddMessageContents(MailMessage mailMessage, SendGridMessage sendGridMessage)
        {
            var contents = new List<Content>();
            if (mailMessage.AlternateViews.Count > 0)
            {
                foreach (var view in mailMessage.AlternateViews)
                {
                    if (view.ContentStream.CanSeek)
                    {
                        view.ContentStream.Position = 0;
                    }

                    var reader = new StreamReader(view.ContentStream);
                    contents.Add(new Content
                    {
                        Type = view.ContentType.MediaType,
                        Value = reader.ReadToEnd()
                    });
                }
            }

            if (contents.Count == 0)
            {
                contents.Add(new Content(mailMessage.IsBodyHtml ? MediaTypeNames.Text.Html : MediaTypeNames.Text.Plain, mailMessage.Body));
            }

            sendGridMessage.Contents = contents;
        }


        /// <summary>
        /// Converts a binary stream to a Base64 string.
        /// </summary>
        /// <param name="source">The binary stream to be converted.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="NotSupportedException"></exception>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="IOException"></exception>
        private string ConvertAttachmentToBase64(Stream source)
        {
            string result = null;
            if (source != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    source.CopyTo(memoryStream);
                    result = Convert.ToBase64String(memoryStream.ToArray());
                }
            }

            return result;
        }


        private bool IsEmailValid(SendGridMessage sendGridMessage)
        {
            return sendGridMessage != null &&
                !String.IsNullOrEmpty(sendGridMessage.Subject) &&
                !String.IsNullOrEmpty(sendGridMessage.From.Email) &&
                sendGridMessage.Personalizations != null &&
                sendGridMessage.Personalizations.Count > 0 &&
                sendGridMessage.Contents != null &&
                sendGridMessage.Contents.Count > 0;
        }
    }
}