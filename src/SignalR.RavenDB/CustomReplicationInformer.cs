using System;
using System.Collections.Generic;
using System.Linq;
using Raven.Client.Connection;
using Raven.Client.Document;

namespace SignalR.RavenDB
{
    class CustomReplicationInformer : ReplicationInformer
    {
        private readonly IList<string> _failOverUrls;

        public CustomReplicationInformer(DocumentConvention conventions, IEnumerable<string> failOverUrls) 
            : base(conventions)
        {
            if (failOverUrls == null)
                throw new ArgumentNullException("failOverUrls");
            _failOverUrls = failOverUrls.ToList();
        }        
    }
}
