using Microsoft.AspNet.SignalR;
using Owin;

namespace SignalR.RavenDB.SampleServer
{
    class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            GlobalHost.DependencyResolver.UseRaven("raven_backplane");

            var config = new HubConfiguration { EnableDetailedErrors = true };            
            app.MapSignalR(config);
        }
    }
}
