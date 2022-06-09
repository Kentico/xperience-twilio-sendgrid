using Kentico.Xperience.Twilio.SendGrid.Models;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using SendGrid;

using System;

namespace Kentico.Xperience.Twilio.SendGrid
{
    /// <summary>
    /// Application startup extension methods.
    /// </summary>
    public static class SendGridStartupExtensions
    {
        /// <summary>
        /// Registers <see cref="ISendGridClient"/> with Dependency Injection.
        /// </summary>
        /// <param name="services">The service collection.</param>
        /// <param name="configuration">The application configuration.</param>
        public static IServiceCollection AddSendGrid(this IServiceCollection services, IConfiguration configuration)
        {
            var sendGridOptions = configuration.GetSection(SendGridOptions.SECTION_NAME).Get<SendGridOptions>();
            if (!String.IsNullOrEmpty(sendGridOptions.ApiKey))
            {
                services.AddSingleton<ISendGridClient>(new SendGridClient(sendGridOptions.ApiKey));
            }

            return services;
        }
    }
}
