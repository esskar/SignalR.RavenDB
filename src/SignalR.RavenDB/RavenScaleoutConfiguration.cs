using System;
using Microsoft.AspNet.SignalR.Messaging;
using Raven.Client;
using Raven.Client.Document;

namespace SignalR.RavenDB
{
    public class RavenScaleoutConfiguration : ScaleoutConfiguration
    {
        private readonly Func<IDocumentStore> _documentStoreFactory;

        public RavenScaleoutConfiguration(string connectionStringNameOrUrl)
            : this(CreateConnectionFactory(connectionStringNameOrUrl)) { }

        public RavenScaleoutConfiguration(Func<IDocumentStore> documentStoreFactory)
        {
            if (documentStoreFactory == null)
                throw new ArgumentNullException("documentStoreFactory");
            _documentStoreFactory = documentStoreFactory;

            this.ReconnectDelay = TimeSpan.FromSeconds(2);
            this.Expiration = TimeSpan.Zero;
        }

        public TimeSpan ReconnectDelay { get; set; }

        public TimeSpan Expiration { get; set; }

        internal Func<IDocumentStore> DocumentStoreFactory
        {
            get { return _documentStoreFactory; }
        }

        private static Func<IDocumentStore> CreateConnectionFactory(string connectionStringNameOrUrl)
        {
            if (string.IsNullOrWhiteSpace(connectionStringNameOrUrl))
                throw new ArgumentException("connectionStringNameOrUrl");

            try
            {
                var uri = new Uri(connectionStringNameOrUrl, UriKind.Absolute);
                return () => new DocumentStore { Url = uri.AbsoluteUri };
            }
            catch (UriFormatException)
            {
                return () => new DocumentStore { ConnectionStringName = connectionStringNameOrUrl };
            }                       
        }
    }
}
