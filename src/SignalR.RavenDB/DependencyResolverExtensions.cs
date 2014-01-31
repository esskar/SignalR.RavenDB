using System;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;

namespace SignalR.RavenDB
{
    public static class DependencyResolverExtensions
    {
        /// <summary>
        /// Use RavenDB as the messaging backplane for scaling out of ASP.NET SignalR applications in a web farm.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="connectionStringNameOrUrl">The connection string name or URL.</param>
        /// <returns></returns>
        public static IDependencyResolver UseRaven(this IDependencyResolver resolver, string connectionStringNameOrUrl)
        {
            var configuration = new RavenScaleoutConfiguration(connectionStringNameOrUrl);
            return resolver.UseRaven(configuration);
        }

        /// <summary>
        /// Use RavenDB as the messaging backplane for scaling out of ASP.NET SignalR applications in a web farm.
        /// </summary>
        /// <param name="resolver">The resolver.</param>
        /// <param name="configuration">The configuration.</param>
        /// <returns></returns>
        public static IDependencyResolver UseRaven(this IDependencyResolver resolver, RavenScaleoutConfiguration configuration)
        {
            var bus = new Lazy<RavenMessageBus>(() => new RavenMessageBus(resolver, configuration));
            resolver.Register(typeof(IMessageBus), () => bus.Value);

            return resolver;
        }
    }
}