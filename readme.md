![Sun Auto Tire & Service](https://white-meadow-0b97e2410.4.azurestaticapps.net/SunAutoLogoBR.png)

# Universal Logging

A service, SaaS-based GUI, and multiple clients for managing logging for various platforms and languages as well as monitoring and managment.

## .NET Logger

A .NET Assembly which uses dependency injection to enhance built in logging provider persisting the log in Azure Data Storage.

### Get Started

1. Connect to feed
	1. Open https://dev.azure.com/SunAuto/Pipelines/_artifacts/feed/SunAuto/connect in the browser.
	1. Follow the instructions for Visual Studio.
1. Reference the package in your .NET assembly
	1. Use package management that is built in w/ Visual Studio making sure that VS is set to see at least the Sun Auto feed.
	1. Or use the command line: `Install-Package SunAuto.Logging`
1. Configure the logger
	1. In your application's settings file (e.g., `appsettings.json`), add the following JSON object in the `Logging` section:
	```JSON
	"Logging": {
		"SunAuto": {
			"Path": "..\\..\\..\\SunAuto.log",
			"LogLevel": {
				"Default": "Trace",
				"Microsoft.Hosting": "Information"
			}
		}
	}
	```
	2. Set your default log level.
	1. Add the `Path` line if you wish to use local file-based logging.
		1. If you use the local file-based logging, then add an `Environment` variable to denote local logging in your start up program file:
		   `Environment.SetEnvironmentVariable("LoggingEnvironment", "Local");`
1. Inject the logger
	1. In the start-up program file, add the following lines:
	```C
	using SunAuto.Logging;

	builder.Logging.ClearProviders();
	builder.Logging.AddLogging();
	```
	Use the first line only if you don't want any other loggers used.
	1. Add the ILoggerFactory logger parameter to any of the service-managed classes (e.g. controllers, etc.)
	```C#
	public class MyClass(ILoggerFactory logger)
	{
		ILogger<MyClass> _Logger = logger.CreateLogger<MyClass>();
	}
    ```

## ReactJS Logger

An npm package which can allow React applications to log to Azure Data Storage.

## PowerShell Logger

A .NET-based cmdlet allowing PowerShell scripts to log to Azure Data Storage.

## Log Viewer

A SaaS-based web application which presents the logged information in Azure Data Storage to any web browser.