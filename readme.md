
# Sun Auto Splunk.NET

A .NET library in a NuGet package allowing convenient logging to Splunk using dependency injection and Microsoft Logging Extensions.

## Get Started

### Install the NuGet package

* Open <https://dev.azure.com/SunAuto/Pipelines/_artifacts/feed/SunAuto/connect> in the browser, and follow the instructions for Visual Studio.
* Reference the package in your .NET assembly
* Use package management that is built in w/ Visual Studio making sure that VS is set to see at least the Sun Auto feed, or use the command line: `Install-Package SunAuto.Logging`.

### Configure the logger

In your application's settings file (e.g., `appsettings.json`), add the following JSON object in the `Logging` section adjusting your logging levels according to your needs:

 ```JSON
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    },
    "SunAuto": {
      "Source": <Name of this resource/API>,
      "BaseUrl": <Base URL from Splunk>,
      "Token": <Splunk token>,
      "LogLevel": {
        "Default": "Information",
        "Microsoft.Hosting": "Warning"
      }
    }
  },
 ```

### Inject the logger

In the start-up program file, add the following lines:

 ```cs
 using SunAuto.Logging.Client;

 builder.Logging.AddSplunkLogging(configuration);

 ```

Use the `ClearProviders()` method only if you don't want any other loggers used.

### Use the Logger

Add the `ILoggerFactory logger` parameter to any of the service-managed classes (e.g. controllers, etc.)

 ```C#
 public class MyClass(ILoggerFactory logger)
 {
    ILogger<MyClass> _Logger = logger.CreateLogger<MyClass>();
 }
```

Then use the built in logging extension methods to create log entries:

```cs
_Logger.LogWarning(exception, "I'm warning you that there is an exception.");
```

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)
## References

* [Logging in C# and .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line)
* [Implement a custom logging provider in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider)
* [Splunk](https://www.splunk.com/)

## Support

If you like this project and think it has helped in any way, consider getting tires or auto service at a Sun Auto Tire & Service location near you:

<a href="https://sun.auto/home" target="_blank"><img src="https://sun.auto/wp-content/themes/sun-auto/images/logo_sunauto.png" alt="Sun Auto Tire & Service" width="150" height="65"/></a>
