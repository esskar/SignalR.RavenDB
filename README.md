# SignalR.RavenDB

RavenDB messaging backplane for scaling out of ASP.NET SignalR applications in a web-farm.

## Get it on NuGet!

```
PM> Install-Package SignalR.RavenDB
```

## Usage

* [Download RavenDB][raven-download] and [run the Server][raven-tutorial]
* Add [Microsoft.AspNet.SignalR][signalr-nuget] NuGet packages to your application
* Create a SignalR application
* Add the following code to Startup.cs to configure the backplane:
```csharp
using Microsoft.AspNet.SignalR;
using SignalR.RavenDB;

public class Startup
{
	public void Configuration(IAppBuilder app)
	{
		// Any connection or hub wire up and configuration should go here
		GlobalHost.DependencyResolver.UseRaven("raven_backplane");
		app.MapSignalR();
	}
}
```
* Add the following code to Web.config
```xml
<configuration>
  <connectionStrings>
    <add name="raven_backplane" connectionString="Url = http://localhost:8080/; Database = signalr" />
  </connectionStrings>
</configuration>
```

[raven-download]: http://ravendb.net/download
[raven-tutorial]: http://ravendb.net/docs/2.5/intro/ravendb-in-a-nutshell
[signalr-nuget]: http://nuget.org/packages/Microsoft.AspNet.SignalR