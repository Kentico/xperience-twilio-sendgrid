[![Stack Overflow][stackoverflow-shield]][stackoverflow-url]
[![Kentico.Xperience.Libraries][xperience-shield]][xperience-url]
[![Nuget](https://img.shields.io/nuget/v/Kentico.Xperience.Twilio.SendGrid)](https://www.nuget.org/packages/Kentico.Xperience.Twilio.SendGrid)

# Xperience SendGrid Integration

This integration allows the dipatching of all Xperience emails from the __Email queue__ to SendGrid using their reliable [Web API](https://sendgrid.com/go/email-api-signup). Using the Web API offers [faster email processing](https://sendgrid.com/blog/web-api-or-smtp-relay-how-should-you-send-your-mail/), so you can squeeze out the most performance while sending those big marketing campaigns!

Additional functionality such as handling SendGrid events and an interface to manage Xperience and SendGrid suppressions will be released at the beginning of July 2022.

## Set up the environment

### Install the NuGet package

1. Install the [Kentico.Xperience.Twilio.SendGrid](https://www.nuget.org/packages/Kentico.Xperience.Twilio.SendGrid) NuGet package in both the administration and the live-site projects.
2. Access your [SendGrid account](https://app.sendgrid.com/), or sign up for a new account [here](https://signup.sendgrid.com/) (it's free to start!).
3. Go to __Settings → API Keys__ and click the _Create API Key_ button.
4. Create an API key with __Restricted Access__ and grant the following permissions:
   - Mail Send: Full access
   - Suppressions: Full access
5. Copy the API key and add the following to your __administration__ project's web.config, in the `appSettings` section:
```xml
<add key="SendGridApiKey" value="<API key>" />
```
6. Add the API key to your __live-site__ project's configuration file:

- __Core__ - _appSettings.json_
```json
"SendGridApiKey": "<API key>",
```
- __MVC__ - _web.config_
```xml
<add key="SendGridApiKey" value="<API key>" />
```

7. In the live-site project's startup code, call the `AddSendGrid()` extension method:
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSendGrid(Configuration);
}
```

## SendGrid email sending

After [importing](#import-the-custom-module) and [configuring](#configure-the-sendgrid-account) the integration, _all_ Xperience emails will be dispatched via SendGrid's [Web API](https://sendgrid.com/go/email-api-signup). When SendGrid successfully receives the email for further processing, the Xperience email will be removed from the __Email queue__ and archived or deleted. If SendGrid cannot successfully queue the email for processing, the Xperience email will remain in the email queue with an error message.

Emails are dispatched to SendGrid by the `ISendGridEmailSender`, of which you can find the default implementation [here](CMSModules/Kentico.Xperience.Twilio.SendGrid/Services/Implementations/DefaultSendGridEmailSender.cs). If you would like to customize email sending, you can create your own implementation of `ISendGridEmailSender` and register it with higher `Priority`:

```cs
[assembly: RegisterImplementation(typeof(ISendGridEmailSender), typeof(MyCustomSendGridEmailSender), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.Default)]
namespace MySite.Twilio.SendGrid {
    /// <summary>
    /// Custom implementation of <see cref="ISendGridEmailSender"/>.
    /// </summary>
    public class MyCustomSendGridEmailSender : ISendGridEmailSender {
        ...
    }
}
```

### SendGrid mail configuration

Within SendGrid's  __Settings__, you can globally configure many [mail settings](https://docs.sendgrid.com/ui/account-and-settings/mail) and [tracking settings](https://docs.sendgrid.com/ui/account-and-settings/tracking). Most of those settings can also be set using SendGrid's API, as seen in the default implementation of `ISendGridConfigurationProvider` [here](/src/Services/Implementations/DefaultSendGridConfigurationProvider.cs). If you would like to fine-tune these settings per email, per marketing campaign, or per Xperience site, you can register your own implementation of `ISendGridConfigurationProvider` with higher `Priority`:

```cs
[assembly: RegisterImplementation(typeof(ISendGridConfigurationProvider), typeof(MyCustomSendGridConfigurationProvider), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.Default)]
namespace MySite.Twilio.SendGrid {
    /// <summary>
    /// Custom implementation of <see cref="ISendGridConfigurationProvider"/>.
    /// </summary>
    public class MyCustomSendGridConfigurationProvider : ISendGridConfigurationProvider {
      ...
    }
  }
```

This could be helpful if, for example, you want to use a different IP Pool for each Xperience site. Or, if you want tracking to be enabled only for certain emails.


## Feedback & Contributing

Did you find a bug, have a feature request, or just want to let us know how we're doing? Create a new [GitHub Issue](https://github.com/Kentico/xperience-twilio-sendgrid/issues/new/choose) and tell us about it! If you'd like to contribute to the project, check out [CONTRIBUTING.md](/CONTRIBUTING.md) to get started.

## License

Distributed under the MIT License. See [LICENSE.md](LICENSE.md) for more information.

## Questions & Support

See the [Kentico home repository](https://github.com/Kentico/Home/blob/master/README.md) for more information about the product(s) and general advice on submitting questions.

[stackoverflow-shield]: https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white
[stackoverflow-url]: https://stackoverflow.com/tags/kentico
[xperience-shield]: https://img.shields.io/badge/Kentico.Xperience.Libraries-v13.0.73-orange
[xperience-url]: https://www.nuget.org/packages/Kentico.Xperience.Libraries
[suppressions-img]: /img/suppressions.png
