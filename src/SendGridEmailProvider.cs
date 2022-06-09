using CMS;
using CMS.Base;
using CMS.Core;
using CMS.EmailEngine;

using Kentico.Xperience.Twilio.SendGrid;
using Kentico.Xperience.Twilio.SendGrid.Services;

using SendGrid;

using System;
using System.ComponentModel;
using System.Net.Mail;

[assembly: RegisterCustomProvider(typeof(SendGridEmailProvider))]
namespace Kentico.Xperience.Twilio.SendGrid
{
    /// <summary>
    /// A custom <see cref="EmailProvider"/> which dispatches emails to SendGrid.
    /// </summary>
    public class SendGridEmailProvider : EmailProvider
    {
        protected override void SendEmailInternal(string siteName, MailMessage message, SMTPServerInfo smtpServer)
        {
            if (Service.ResolveOptional<ISendGridClient>() == null)
            {
                return;
            }

            var sendGridEmailSender = Service.Resolve<ISendGridEmailSender>();
            sendGridEmailSender.SendEmail(message, siteName);
        }


        protected override void SendEmailAsyncInternal(string siteName, MailMessage message, SMTPServerInfo smtpServer, EmailToken emailToken)
        {
            if (Service.ResolveOptional<ISendGridClient>() == null)
            {
                OnSendCompleted(new AsyncCompletedEventArgs(new Exception("SendGrid client is not configured properly."), true, emailToken));
                return;
            }

            var sendGridEmailSender = Service.Resolve<ISendGridEmailSender>();
            new CMSThread(() =>
            {
                var asyncCompletedEventArgs = sendGridEmailSender.SendEmail(message, siteName, emailToken);
                OnSendCompleted(asyncCompletedEventArgs);
            },
            new ThreadSettings
            {
                UseEmptyContext = true,
                Mode = ThreadModeEnum.Async
            })
            .Start();
        }
    }
}