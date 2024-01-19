
![Sun Auto Tire & Service](https://white-meadow-0b97e2410.4.azurestaticapps.net/SunAutoLogoBR.png)

# Universal Logging

A service, SaaS-based GUI, and multiple clients for managing logging for various platforms and languages as well as monitoring and managment.

## .NET Logger

A .NET Assembly which uses dependency injection to enhance built in logging provider persisting the log in Azure Data Storage.

### Get Started

#### Connect to feed

* Open <https://dev.azure.com/SunAuto/Pipelines/_artifacts/feed/SunAuto/connect> in the browser, and follow the instructions for Visual Studio.
* Reference the package in your .NET assembly
* Use package management that is built in w/ Visual Studio making sure that VS is set to see at least the Sun Auto feed, or use the command line: `Install-Package SunAuto.Logging`.

#### Configure the logger

In your application's settings file (e.g., `appsettings.json`), add the following JSON object in the `Logging` section:

 ```JSON
  "SunAuto": {
   "Path": "..\\..\\..\\SunAuto.log",
   "LogLevel": {
    "Default": "Trace",
    "Microsoft.Hosting": "Information"
   }
  }
 ```

1. Set your default log level (e.g. Trace, Information, etc.).
1. Add the `Path` line if you wish to use local file-based logging.
1. If you use the local file-based logging, then add an `Environment` variable to denote local logging in your start up program file:
     `Environment.SetEnvironmentVariable("LoggingEnvironment", "Local");`

#### Inject the logger

In the start-up program file, add the following lines:

 ```cs
 using SunAuto.Logging;

 builder.Logging.ClearProviders();
 builder.Logging.AddLogging();
 ```

Use the `ClearProviders()` method only if you don't want any other loggers used.

#### Use the Logger

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

## ReactJS Logger

An npm package which can allow React applications to log to Azure Data Storage.

## PowerShell Logger

A .NET-based cmdlet allowing PowerShell scripts to log to Azure Data Storage.

> This feature is on hold until it can be determined that there is long term support (LTS) for powershell cmdlet development in .NET >=8.0.

## Log Viewer

A SaaS-based web application which presents the logged information in Azure Data Storage to any web browser.

## References

* [Logging in C# and .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/logging?tabs=command-line)
* [Implement a custom logging provider in .NET](https://learn.microsoft.com/en-us/dotnet/core/extensions/custom-logging-provider)