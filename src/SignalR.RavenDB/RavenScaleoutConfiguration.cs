using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using Microsoft.AspNet.SignalR.Messaging;
using Raven.Client;
using Raven.Client.Document;

namespace SignalR.RavenDB
{
    public class RavenScaleoutConfiguration : ScaleoutConfiguration
    {
        private readonly Func<IDocumentStore> _documentStoreFactory;

        public static RavenScaleoutConfiguration CreateWithFailover(string connectionStringName)
        {
            if (string.IsNullOrWhiteSpace(connectionStringName))
                throw new ArgumentNullException("connectionStringName");

            var defaultConnectionString = GetConnectionStrings(connectionStringName, false).FirstOrDefault();
            if (string.IsNullOrWhiteSpace(defaultConnectionString))
                throw new ArgumentException("No connection string found under given name.", "connectionStringName");

            var uris = new List<string> { defaultConnectionString };
            uris.AddRange(GetConnectionStrings(connectionStringName + "_failover", true));
            return new RavenScaleoutConfiguration(uris);
        }
        
        public RavenScaleoutConfiguration(string connectionStringName)
            : this(CreateConnectionFactoryFromConnectionStringNameOrUrl(connectionStringName)) { }

        public RavenScaleoutConfiguration(IEnumerable<string> connectionStringUrls)
            : this(CreateConnectionFactoryFromUrls(connectionStringUrls)) { }

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

        private static Func<IDocumentStore> CreateConnectionFactoryFromUrls(IEnumerable<string> connectionStringUrls)
        {
            if (connectionStringUrls == null)
                throw new ArgumentNullException("connectionStringUrls");

            return () =>
            {
                var documentStore = new DocumentStore();
                documentStore.Conventions.ReplicationInformerFactory = s => new CustomReplicationInformer(documentStore.Conventions, connectionStringUrls);
                return documentStore;
            };
        }

        private static Func<IDocumentStore> CreateConnectionFactoryFromConnectionStringNameOrUrl(string connectionStringNameOrUrl)
        {
            if (string.IsNullOrWhiteSpace(connectionStringNameOrUrl))
                throw new ArgumentNullException("connectionStringNameOrUrl");

            try
            {
                if (!Uri.IsWellFormedUriString(connectionStringNameOrUrl, UriKind.Absolute))
                    return CreateConnectionFactoryFromConnectionStringName(connectionStringNameOrUrl);

                var uri = new Uri(connectionStringNameOrUrl, UriKind.Absolute);
                return () => new DocumentStore { Url = uri.AbsoluteUri };
            }
            catch (UriFormatException)
            {
                return CreateConnectionFactoryFromConnectionStringName(connectionStringNameOrUrl);
            }
        }

        private static Func<IDocumentStore> CreateConnectionFactoryFromConnectionStringName(string connectionStringName)
        {
            return () => new DocumentStore { ConnectionStringName = connectionStringName };
        }

        private static IEnumerable<string> GetConnectionStrings(string name, bool isPrefix)
        {
            for (var i = 0; i < ConfigurationManager.ConnectionStrings.Count; i++)
            {
                var settings = ConfigurationManager.ConnectionStrings[i];
                if (!isPrefix)
                {
                    if (settings.Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        yield return settings.ConnectionString;
                        yield break;
                    }
                }
                else
                {
                    if (settings.Name.StartsWith(name, StringComparison.OrdinalIgnoreCase))
                        yield return settings.ConnectionString;
                }
            }
        }
    }
}
