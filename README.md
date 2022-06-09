[![Stack Overflow][stackoverflow-shield]][stackoverflow-url]
[![Kentico.Xperience.Libraries][xperience-shield]][xperience-url]
[![Nuget](https://img.shields.io/nuget/v/Kentico.Xperience.Twilio.SendGrid)](https://www.nuget.org/packages/Kentico.Xperience.Twilio.SendGrid)

# Twilio SendGrid + Xperience 

This integration allows the dipatching of all Xperience emails from the __Email queue__ to SendGrid using their reliable [Web API](https://sendgrid.com/go/email-api-signup). Using the Web API offers [faster email processing](https://sendgrid.com/blog/web-api-or-smtp-relay-how-should-you-send-your-mail/), so you can squeeze out the most performance while sending those big marketing campaigns! The sending process is highly customizable and you can even use standard Xperience event handlers to react to SendGrid events such as email drops and bounces, or opens and clicks.

## Set up the environment

### Install the NuGet package

1. Install the [Kentico.Xperience.Twilio.SendGrid](https://www.nuget.org/packages/Kentico.Xperience.Twilio.SendGrid) NuGet package in both the administration and the live-site project.
2. Access your [SendGrid account](https://app.sendgrid.com/), or sign up for a new account [here](https://signup.sendgrid.com/) (it's free to start!).
3. Go to __Settings → API Keys__ and click the _Create API Key_ button.
4. Create an API key with __Restricted Access__ and grant the following permissions:
  - Mail Send: Full access
  - Suppressions: Full access
5. Copy the API key and add the following to your CMS project's web.config, in the `appSettings` section:
```xml
<add key="SendGridApiKey" value="<API key>" />
```
6. Add the API key to your live-site project's `appsettings.json` file:
```json
"xperience.twilio.sendgrid": {
  "apiKey": "<API key>"
}
```
7. In the live-site project's startup code, call the `AddSendGrid()` extension method:
```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddSendGrid(Configuration);
}
```

### (Optional) Import the custom module

You may choose to import a custom module into your Xperience administration website to enable new functionality:

- [SendGrid Event Webhooks](#handling-sendgrid-event-webhooks)
- [Suppression management](#suppression-management)

If you would like to use these features, follow these steps to install the custom module:

1. Open your CMS project in __Visual Studio__.
1. Download the latest _"Kentico.Xperience.Twilio.SendGrid"_ ZIP package from the [Releases](https://github.com/Kentico/xperience-twilio-sendgrid/releases/).
1. In the Xperience adminstration, open the __Sites__ application.
1. [Import](https://docs.xperience.io/deploying-websites/exporting-and-importing-sites/importing-a-site-or-objects) the downloaded package with the __Import files__ and __Import code files__ [settings](https://docs.xperience.io/deploying-websites/exporting-and-importing-sites/importing-a-site-or-objects#Importingasiteorobjects-Import-Objectselectionsettings) enabled.
1. Perform the [necessary steps](https://docs.xperience.io/deploying-websites/exporting-and-importing-sites/importing-a-site-or-objects#Importingasiteorobjects-Importingpackageswithfiles) to include the following imported folder in your project:
   - `/CMSModules/Kentico.Xperience.Twilio.SendGrid`

## SendGrid email sending

After [installing](#install-the-nuget-package) the integration on the CMS and live-site applications, _all_ Xperience emails will be dispatched via SendGrid's [Web API](https://sendgrid.com/go/email-api-signup). When SendGrid successfully receives the email for further processing, the Xperience email will be removed from the __Email queue__ and archived or deleted. If SendGrid cannot successfully queue the email for processing, the Xperience email will remain in the queue with an error message.

Emails are dispatched to SendGrid by the `ISendGridEmailSender`, of which you can find the default implementation [here](src/Services/Implementations/DefaultSendGridEmailSender.cs). If you would like to customize email sending, you can create your own implementation of `ISendGridEmailSender` and register it with higher `Priority`:

```cs
[assembly: RegisterImplementation(typeof(ISendGridEmailSender), typeof(MyCustomSendGridEmailSender), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.Default)]
namespace MySite.Twilio.SendGrid {
    /// <summary>
    /// Custom implementation of <see cref="ISendGridEmailSender"/>.
    /// </summary>
    public class MyCustomSendGridEmailSender : ISendGridEmailSender {
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
```

This could be helpful if, for example, you want to use a different IP Pool for each Xperience site. Or, if you want tracking to be enabled only for certain emails.

## Handling SendGrid Event Webhooks

> __Note__ Requires installation of the [custom module](#optional-import-the-custom-module).

SendGrid has the ability to send webhooks to your Xperience administration website when [certain events](https://docs.sendgrid.com/for-developers/tracking-events/event#delivery-events) occur. SendGrid event handling is implemented via standard [Xperience event handling](https://docs.xperience.io/custom-development/handling-global-events), so you can handle SendGrid event webhooks like this:

```cs
SendGridEvents.Bounce.After += HandleSendGridBounce;
```

You can view the available events in the [`SendGridEvents`](src/Events/SendGridEvents.cs) class. To enable event webhook handling in your Xperience administration website:

1. In SendGrid, open __Settings → Mail Settings → Event Webhook__.
2. Set the following values:
  - Authorization Method: None.
  - HTTP Post URL: _https://[your Xperience CMS]/xperience-sendgrid/events_.
  - Events to be POSTed to your URL: Select any events you'd like to handle.
  - Event Webhook Status: Enabled.
3. In __Mail settings__, click __Signed Event Webhook Requests__.
4. Enable _Signed Event Webhook Request Status_ and copy the __Verification Key__.
5. In the CMS project's web.config `appSettings` section, add the following setting:
```xml
<add key="SendGridWebhookVerificationKey" value="<Verification key>"/>
```

## Suppression management

> __Note__ Requires installation of the [custom module](#optional-import-the-custom-module).

Within the Xperience __Email marketing__ application you will find a new tab called _"Suppressions."_ This interface allows you to manage Xperience and SendGrid email suppressions for subscribers of the newsletter or email campaign. For newsletters, the tab can be found when editing the newsletter. For email campaigns, it appears when editing an individual email of the campaign.

![suppressions-img]

The __Bounced in SendGrid__ column indicates whether the email is listed under SendGrid's __Suppressions → Bounces__ list. The __Bounces in Xperience__ column lists the number of bounces recorded in the Xperience database, and will be red if the number of bounces exceeds the _"Bounced email limit"_ setting in __Settings → On-line marketing → Email marketing__. Using the checkboxes and drop-down menu at the bottom of the grid, you can reset these bounces to ensure that your emails are delivered to the recipients.

## Feedback & Contributing

Did you find a bug, have a feature request, or just want to let us know how we're doing? Create a new [GitHub Issue](https://github.com/Kentico/xperience-twilio-sendgrid/issues/new/choose) and tell us about it! If you'd like to contribute to the project, check out [CONTRIBUTING.md](/CONTRIBUTING.md) to get started.

## License

Distributed under the MIT License. See [LICENSE.md](LICENSE.md) for more information.

[stackoverflow-shield]: https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white
[stackoverflow-url]: https://stackoverflow.com/tags/kentico
[xperience-shield]: https://img.shields.io/badge/Kentico.Xperience.Libraries-v13.0.0-orange
[xperience-url]: https://www.nuget.org/packages/Kentico.Xperience.Libraries
[suppressions-img]: /img/suppressions.png