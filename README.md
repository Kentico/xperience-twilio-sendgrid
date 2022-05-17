[![Stack Overflow][stackoverflow-shield]][stackoverflow-url]
[![Kentico.Xperience.Libraries][xperience-shield]][xperience-url]

# Twilio SendGrid + Xperience 

This integration allows the dipatching of all Xperience emails from the __Email queue__ to SendGrid using their reliable [Web API](https://sendgrid.com/go/email-api-signup). Using the Web API offers [faster email processing](https://sendgrid.com/blog/web-api-or-smtp-relay-how-should-you-send-your-mail/), so you can squeeze out the most performance while sending those big marketing campaigns! The sending process is highly customizable and you can even use standard Xperience event handlers to react to SendGrid events such as email drops and bounces, or opens and clicks.

## Set up the environment

### Import the custom module

1. Open your CMS project in __Visual Studio__.
1. Install the _SendGrid_ NuGet package in the CMS project.
1. Download the latest _"Kentico.Xperience.Twilio.SendGrid"_ ZIP package from the [Releases](https://github.com/Kentico/xperience-twilio-sendgrid/releases/).
1. In the Xperience adminstration, open the __Sites__ application.
1. [Import](https://docs.xperience.io/deploying-websites/exporting-and-importing-sites/importing-a-site-or-objects) the downloaded package with the __Import files__ and __Import code files__ [settings](https://docs.xperience.io/deploying-websites/exporting-and-importing-sites/importing-a-site-or-objects#Importingasiteorobjects-Import-Objectselectionsettings) enabled.
1. Perform the [necessary steps](https://docs.xperience.io/deploying-websites/exporting-and-importing-sites/importing-a-site-or-objects#Importingasiteorobjects-Importingpackageswithfiles) to include the following imported folder in your project:
   - `/CMSModules/Kentico.Xperience.Twilio.SendGrid`

### Configure the SendGrid account

1. Access your [SendGrid account](https://app.sendgrid.com/), or sign up for a new account [here](https://signup.sendgrid.com/) (it's free to start!).
2. Go to __Settings → API Keys__ and click the _Create API Key_ button.
3. Create an API key with __Restricted Access__ and grant the following permissions:
  - Mail Send: Full access
  - Suppressions: Full access
4. Copy the API key and add the following to your CMS project's web.config, in the `appSettings` section:
```xml
<add key="SendGridApiKey" value="<API key>" />
```

### Enable SendGrid Event Webhooks

This is _optional_, but required if you'd like to handle [SendGrid Event Webhooks](#sendgrid-event-webhooks) in Xperience, and enable the default "bounce" and "drop" handling in this integration.

1. In SendGrid, open __Settings → Mail Settings → Event Webhook__.
2. Set the following values:
  - Authorization Method: None.
  - HTTP Post URL: _https://[your Xperience CMS]/xperience-sendgrid/events_.
  - Events to be POSTed to your URL: Select at least __Dropped__ and __Bounced__, and any other events you'd like to handle.
  - Event Webhook Status: Enabled.
3. In __Mail settings__, click __Signed Event Webhook Requests__.
4. Enable _Signed Event Webhook Request Status_ and copy the __Verification Key__.
5. In the CMS project's web.config `appSettings` section, add the following setting:
```xml
<add key="SendGridWebhookVerificationKey" value="<Verification key>"/>
```

## SendGrid email sending

After [installing](#import-the-custom-module) and [configuring](#configure-the-sendgrid-account) the integration, _all_ Xperience emails will be dispatched via SendGrid's [Web API](https://sendgrid.com/go/email-api-signup). When SendGrid successfully receives the email for further processing, the Xperience email will be removed from the __Email queue__ and archived or deleted. If SendGrid cannot successfully queue the email for processing, the Xperience email will remain in the queue with an error message.

Emails are dispatched to SendGrid by the `ISendGridEmailSender`, of which you can find the default implementation [here](CMSModules/Kentico.Xperience.Twilio.SendGrid/Services/Implementations/DefaultSendGridEmailSender.cs). If you would like to customize email sending, you can create your own implementation of `ISendGridEmailSender` and register it with higher `Priority`:

```cs
[assembly: RegisterImplementation(typeof(ISendGridEmailSender), typeof(MyCustomSendGridEmailSender), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.Default)]
namespace MySite.Twilio.SendGrid {
    /// <summary>
    /// Custom implementation of <see cref="ISendGridEmailSender"/>.
    /// </summary>
    public class MyCustomSendGridEmailSender : ISendGridEmailSender {
```

### Email settings

Within SendGrid's  __Settings__, you can globally configure many [mail settings](https://docs.sendgrid.com/ui/account-and-settings/mail) and [tracking settings](https://docs.sendgrid.com/ui/account-and-settings/tracking). If you would like to fine-tune these settings per email, per marketing campaign, or per Xperience site, you can use a custom `ISendGridConfigurationProvider` or the [default implementation](CMSModules/Kentico.Xperience.Twilio.SendGrid/Services/Implementations/DefaultSendGridConfigurationProvider.cs). Our implementation allows you to globally configure the following settings via web.config `appSettings`:

| **Application setting** 	| **Value type** 	| **Usage** 	|
|---	|---	|---	|
| **Mail settings**
| SendGridSandboxMode 	| bool 	| Allows you to send [test emails](https://docs.sendgrid.com/for-developers/sending-email/sandbox-mode) to SendGrid which are never delivered. 	|
| SendGridBypassGroupsAndSuppressions 	| bool 	| Bypass all [suppression lists](https://docs.sendgrid.com/ui/sending-email/index-suppressions#bypass-suppressions) to ensure the email is delievered to the recipient. 	|
| SendGridBypassSpam 	| bool 	| Bypass the spam report list. The bounce and global unsubscribe lists will be checked and respected. 	|
| SendGridBypassGlobalUnsubscribes 	| bool 	| Bypass the global unsubscribe list. The spam report and bounce lists will be checked and respected. Note that this bypass filter is applied to global unsubscribes only; group unsubscribes will be checked and respected. 	|
| SendGridBypassBounces 	| bool 	| Bypass the bounce list. The spam report and global unsubscribe lists will be checked and respected. 	|
| **Tracking settings**
| SendGridEnableClickTracking 	| bool 	| Enables tracking whether a recipient clicked a link in your email. 	|
| SendGridEnableOpenTracking 	| bool 	| Enables tracking whether the email was opened or not. 	|
| SendGridEnableSubscriptionTracking 	| bool 	| Enables a subscription management link at the bottom of the text and HTML bodies of the email. 	|
| SendGridEnableGoogleAnalytics 	| bool 	| Enables tracking provided by Google Analytics. 	|
| **Other**
| SendGridIpPoolName 	| string 	| Sets the name of the [IP Pool](https://docs.sendgrid.com/ui/account-and-settings/ip-pools) used for sending the email. 	|
| SendGridDebugEnabled | bool | If enabled, the full JSON of sent emails and SendGrid Event Webhooks are logged to the Event Log. |

You can customize how the settings are retrieved during sending by implementing your own `ISendGridConfigurationProvider`:

```cs
[assembly: RegisterImplementation(typeof(ISendGridConfigurationProvider), typeof(MyCustomSendGridConfigurationProvider), Lifestyle = Lifestyle.Singleton, Priority = RegistrationPriority.Default)]
namespace MySite.Twilio.SendGrid {
    /// <summary>
    /// Custom implementation of <see cref="ISendGridConfigurationProvider"/>.
    /// </summary>
    public class MyCustomSendGridConfigurationProvider : ISendGridConfigurationProvider {
```

This could be helpful if, for example, you want to use a different IP Pool for each Xperience site. Or, if you want tracking to be enabled only for certain emails.

## SendGrid Event Webhooks

SendGrid has the ability to send webhooks to your Xperience administration website when [certain events](https://docs.sendgrid.com/for-developers/tracking-events/event#delivery-events) occur. Within this integration, you can automatically increase an Xperience contact's bounces when a SendGrid email bounces, and a marketing email's __Delivery rate__ will be correctly adjusted when certain events occurs. To enable this functionality, you must [enable SendGrid Event Webhooks](#enable-sendgrid-event-webhooks).

> :warning: When a delayed bounce occurs, the Xperience email headers are lost and the email cannot be associated with the newsletter or email campaign. In these rare cases, the __Delivery rate__ will not be 100% accurate.

SendGrid event handling is implemented via standard [Xperience event handling](https://docs.xperience.io/custom-development/handling-global-events), using custom handlers created for this integration:

```cs
SendGridEvents.Bounce.After += MarkIssueUndelivered;
SendGridEvents.Dropped.After += MarkIssueUndelivered;
SendGridEvents.Blocked.After += MarkIssueUndelivered;
```

You can view the available events in the [`SendGridEvents`](CMSModules/Kentico.Xperience.Twilio.SendGrid/Events/SendGridEvents.cs) class. If you wish to handle any of these events, you must ensure that the SendGrid Event Webhook is enabled, as described in __step #2__ of [Enable SendGrid Event Webhooks](#enable-sendgrid-event-webhooks).

## Suppression management

Within the Xperience __Email marketing__ application you will find a new tab called _"Suppressions."_ This interface allows you to manage Xperience and SendGrid email suppressions for subscribers of the newsletter or email campaign. For newsletters, the tab can be found when editing the newsletter. For email campaigns, it appears when editing an individual email of the campaign.

![suppressions-img]

The __Bounced in SendGrid__ column indicates whether the email is listed under SendGrid's __Suppressions → Bounces__ list. The __Bounces in Xperience__ column lists the number of bounces recorded in the Xperience database, and will be red if the number of bounces exceeds the _"Bounced email limit"_ setting in __Settings → On-line marketing → Email marketing__. Using the checkboxes and drop-down menu at the bottom of the grid, you can delete these suppressions to ensure that your emails are delivered to the recipients.

## Feedback & Contributing

Did you find a bug, have a feature request, or just want to let us know how we're doing? Create a new [GitHub Issue](https://github.com/Kentico/xperience-twilio-sendgrid/issues/new/choose) and tell us about it! If you'd like to contribute to the project, check out [CONTRIBUTING.md](/CONTRIBUTING.md) to get started.

## License

Distributed under the MIT License. See [LICENSE.md](LICENSE.md) for more information.

[stackoverflow-shield]: https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white
[stackoverflow-url]: https://stackoverflow.com/tags/kentico
[xperience-shield]: https://img.shields.io/badge/Kentico.Xperience.Libraries-v13.0.0-orange
[xperience-url]: https://www.nuget.org/packages/Kentico.Xperience.Libraries
[suppressions-img]: /Assets/suppressions.png