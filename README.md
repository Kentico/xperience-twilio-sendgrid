[![Stack Overflow][stackoverflow-shield]][stackoverflow-url]
[![Kentico.Xperience.Libraries][xperience-shield]][xperience-url]

# Twilio SendGrid + Xperience 

This integration allows the dipatching of all Xperience emails from the __Email queue__ to SendGrid using their reliable [Web API](https://sendgrid.com/go/email-api-signup). Using the Web API offers [faster email processing](https://sendgrid.com/blog/web-api-or-smtp-relay-how-should-you-send-your-mail/), so you can squeeze out the most performance while sending those big marketing campaigns! The sending process is highly customizable and you can even use standard Xperience event handlers to react to SendGrid events such as email drops and bounces, or opens and clicks.

## Set up the environment

### Import the custom module

1. Open your CMS project in __Visual Studio__.
1. Install the _SendGrid_ NuGet packages in the CMS project.
1. Download the latest ZIP package from the [Releases](https://github.com/Kentico/xperience-twilio-sendgrid/releases/).
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

### Enable SendGrid webhook events

1. In SendGrid, open __Settings → Mail Settings → Event Webhook__.
2. Set the following values:
  - Authorization Method: None.
  - HTTP Post URL: _https://[your Xperience website]/xperience-sendgrid/events_.
  - Events to be POSTed to your URL: Select at least __Dropped__ and __Bounced__, and any other events you'd like to handle.
  - Event Webhook Status: Enabled.
3. In __Mail settings__, click __Signed Event Webhook Requests__.
4. Enable _Signed Event Webhook Request Status_ and copy the __Verification Key__.
5. In the CMS project's web.config `appSettings` setting, add the following setting:
```xml
<add key="SendGridWebhookVerificationKey" value="<Verification key>"/>
```



## Contributing

For Contributing please see [CONTRIBUTING.md](CONTRIBUTING.md) for more information.

## License

Distributed under the MIT License. See [LICENSE.md](LICENSE.md) for more information.

[stackoverflow-shield]: https://img.shields.io/badge/Stack%20Overflow-ASK%20NOW-FE7A16.svg?logo=stackoverflow&logoColor=white
[stackoverflow-url]: https://stackoverflow.com/tags/kentico
[xperience-shield]: https://img.shields.io/badge/Kentico.Xperience.Libraries-v13.0.0-orange
[xperience-url]: https://www.nuget.org/packages/Kentico.Xperience.Libraries