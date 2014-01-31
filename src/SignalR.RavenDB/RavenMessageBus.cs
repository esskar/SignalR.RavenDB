using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Tracing;
using Raven.Client;

namespace SignalR.RavenDB
{
    public class RavenMessageBus : ScaleoutMessageBus
    {
        private readonly TraceSource _trace;
        private readonly Func<IDocumentStore> _documentStoreFactory;

        private int _state;
        private IDocumentStore _documentStore;

        public RavenMessageBus(IDependencyResolver resolver, RavenScaleoutConfiguration configuration) 
            : base(resolver, configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _documentStoreFactory = configuration.DocumentStoreFactory;

            // initialize trace source
            var traceManager = resolver.Resolve<ITraceManager>();
            _trace = traceManager["SignalR." + typeof(RavenMessageBus).Name];
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var oldState = Interlocked.Exchange(ref _state, State.Disposing);
                switch (oldState)
                {
                    case State.Connected:
                        this.Shutdown();
                        break;
                    case State.Disposed:
                        Interlocked.Exchange(ref _state, State.Disposed);
                        break;
                }
            }

            base.Dispose(disposing);
        }

        private Task Connect()
        {
            if (_documentStore != null)
            {
                _documentStore.Dispose();
                _documentStore = null;
            }

            try
            {
                _trace.TraceInformation("Initializing connection ...");

                var documentStore = _documentStoreFactory();
                documentStore.Initialize();

                _trace.TraceInformation("Connection initialized.");

                //TODO: Subscribe


                _documentStore = documentStore;
                return TaskAsyncHelper.Empty;
            }
            catch (Exception ex)
            {
                _trace.TraceError("Error connecting to RavenDB - " + ex.GetBaseException());
                return TaskAsyncHelper.FromError(ex);
            }
        }

        private void Shutdown()
        {
            _trace.TraceInformation("Shutdown()");

            /*
            if (_channel != null)
            {
                _channel.Unsubscribe(_key);
                _channel.Close(abort: true);
            }
            */
            if (_documentStore != null)
            {
                _documentStore.Dispose();
                _documentStore = null;
            }

            Interlocked.Exchange(ref _state, State.Disposed);
        }

        private static class State
        {
            public const int Closed = 0;
            public const int Connected = 1;
            public const int Disposing = 2;
            public const int Disposed = 3;
        }        
    }
}
