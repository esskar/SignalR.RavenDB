# SignalR.RavenDB [![Build status](https://ci.appveyor.com/api/projects/status?id=r1nc0ga4fwco48cq)](https://ci.appveyor.com/project/signalr-ravendb)

RavenDB messaging backplane for scaling out of ASP.NET SignalR applications in a web-farm.

## Get it on NuGet!

SignalR.RavenDB is available via [NuGet][me-nuget].

```
PM> Install-Package SignalR.RavenDB
```

## Usage

* [Download RavenDB][raven-download] and [run the Server][raven-tutorial]
* Add [Microsoft.AspNet.SignalR][signalr-nuget] NuGet package to your application
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
* Add the following lines to Web.config
```xml
<configuration>
  <connectionStrings>
    <add name="raven_backplane" connectionString="Url = http://localhost:8080/; Database = signalr" />
  </connectionStrings>
</configuration>
```

### Setting up the Database

RavenDB is easy to use. It will also create the database for you if it does not exist when you first try to connect to it.
This will probably fine during development cases. But you may want to tweak it a bit before going live.

#### Compression

RavenDB supports data compression. To be able to use that feature you have to enable to [compression bundle][raven-compression] during the database creation process.
This will reduce package size and will probably increase your throughtput.

#### Expiration

The [expiration bundle][raven-expiration] serves a very simple purpose, it deletes documents whose time have passed.
You may use this bundle to autmatically delete old messages from the bus.

You also have to tell SignalR.RavenDB to use expiration by setting the `Expiration` property during configuration:
```csharp
using Microsoft.AspNet.SignalR;
using SignalR.RavenDB;

public class Startup
{
	public void Configuration(IAppBuilder app)
	{
		// Any connection or hub wire up and configuration should go here
		GlobalHost.DependencyResolver.UseRaven(new RavenScaleoutConfiguration("raven_backplane")
		{
		    Expiration = TimeSpan.FromMinutes(10)
		});
		app.MapSignalR();
	}
}
```

*WARNING:* When master-master replication is set between RavenDB servers then the Expiration bundle should be turned on ONLY on one server, 
otherwise conflicts will occur.

[raven-download]: http://ravendb.net/download
[raven-tutorial]: http://ravendb.net/docs/2.5/intro/ravendb-in-a-nutshell
[raven-compression]: http://ravendb.net/docs/2.0/server/extending/bundles/compression
[raven-expiration]: http://ravendb.net/docs/2.0/server/extending/bundles/expiration
[signalr-nuget]: http://nuget.org/packages/Microsoft.AspNet.SignalR
[me-nuget]: http://www.nuget.org/packages/SignalR.RavenDB
