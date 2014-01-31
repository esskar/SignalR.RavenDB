using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Tracing;

namespace SignalR.RavenDB
{
    public class RavenMessageBus : ScaleoutMessageBus
    {
        private readonly TraceSource _trace;

        private int _state;

        public RavenMessageBus(IDependencyResolver resolver, ScaleoutConfiguration configuration) 
            : base(resolver, configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

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

        private void Shutdown()
        {
            _trace.TraceInformation("Shutdown()");

            /*
            if (_channel != null)
            {
                _channel.Unsubscribe(_key);
                _channel.Close(abort: true);
            }

            if (_connection != null)
            {
                _connection.Close(abort: true);
            }*/

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
