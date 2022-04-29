using CMS.EmailEngine;

using SendGrid.Helpers.Mail;

using System.ComponentModel;
using System.Net.Mail;

namespace Kentico.Xperience.Twilio.SendGrid.Services
{
    /// <summary>
    /// Contains methods for dispatching Xperience emails to SendGrid via web API.
    /// </summary>
    public interface ISendGridEmailSender
    {
        /// <summary>
        /// Converts an email from Xperience into a <see cref="SendGridMessage"/> to be dispatched.
        /// </summary>
        /// <param name="mailMessage">The Xperience email to be converted.</param>
        /// <param name="siteName">The Xperience site name.</param>
        /// <returns>The SendGrid email, or null if there were errors during conversion.</returns>
        SendGridMessage ConvertMailMessage(MailMessage mailMessage, string siteName);


        /// <summary>
        /// Dispatches an Xperience email to SendGrid.
        /// </summary>
        /// <param name="message">The Xperience email to be dispatched.</param>
        /// <param name="siteName">The Xperience site name.</param>
        /// <param name="emailToken">Email token that represents the email being sent.</param>
        /// <returns>The results of the email sending.</returns>
        AsyncCompletedEventArgs SendEmail(MailMessage message, string siteName, EmailToken emailToken = null);
    }
}