using System;
using Raven.Abstractions.Data;

namespace SignalR.RavenDB
{
    internal class RavenMessageObserver : IObserver<DocumentChangeNotification>, IDisposable
    {
        private RavenMessageBus _bus;

        public RavenMessageObserver(RavenMessageBus bus)
        {
            if (bus == null)
                throw new ArgumentNullException("bus");
            _bus = bus;
        }

        public void Dispose()
        {
            _bus = null;
        }

        void IObserver<DocumentChangeNotification>.OnNext(DocumentChangeNotification value)
        {
            _bus.TraceVerbose("Document change notification of type '{0}' with id '{1}' received: {2}", 
                value.Type, value.Id, value.Message);
            if (value.Type != DocumentChangeTypes.Put)
                return;

            _bus.OnMessage(value.Id);
        }

        void IObserver<DocumentChangeNotification>.OnError(Exception error)
        {
            _bus.TraceError("Observer detected an error - {0}", error.GetBaseException());
        }

        void IObserver<DocumentChangeNotification>.OnCompleted()
        {
            _bus.TraceInformation("Observer completed.");
        }
    }
}
