using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Messaging;
using Microsoft.AspNet.SignalR.Tracing;
using Raven.Client;
using Raven.Client.Changes;
using Raven.Json.Linq;

namespace SignalR.RavenDB
{
    public class RavenMessageBus : ScaleoutMessageBus
    {
        private const string RavenExpirationDate = "Raven-Expiration-Date";

        private readonly TraceSource _trace;
        private readonly Func<IDocumentStore> _documentStoreFactory;
        private readonly RavenMessageObserver _observer;
        private readonly object _callbackLock = new object();
        private readonly TimeSpan _reconnectDelay;
        private readonly TimeSpan _expiration;

        private int _state;
        private IDocumentStore _documentStore;
        private IDatabaseChanges _databaseChanges;
        private IDisposable _subscription;

        public RavenMessageBus(IDependencyResolver resolver, RavenScaleoutConfiguration configuration)
            : base(resolver, configuration)
        {
            if (configuration == null)
                throw new ArgumentNullException("configuration");

            _documentStoreFactory = configuration.DocumentStoreFactory;
            _reconnectDelay = configuration.ReconnectDelay;
            _expiration = configuration.Expiration;

            // initialize trace source
            var traceManager = resolver.Resolve<ITraceManager>();
            _trace = traceManager["SignalR." + typeof(RavenMessageBus).Name];

            _observer = new RavenMessageObserver(this);            
            this.ConnectWithRetry();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                var oldState = Interlocked.Exchange(ref _state, State.Disposing);
                switch (oldState)
                {
                    case State.Connected:
                    case State.Closed:
                        this.Shutdown();
                        break;
                    case State.Disposed:
                        Interlocked.Exchange(ref _state, State.Disposed);
                        break;
                }
            }

            base.Dispose(disposing);
        }

        protected override Task Send(int streamIndex, IList<Message> messages)
        {
            _trace.TraceVerbose("Send called with stream index {0}.", streamIndex);

            var tcs = new TaskCompletionSource<object>();
            this.SendMessages(streamIndex, messages, tcs);
            return tcs.Task;
        }

        private async void SendMessages(int streamIndex, IList<Message> messages, TaskCompletionSource<object> tcs)
        {
            try
            {
                var ravenMessage = RavenMessage.FromMessages(streamIndex, messages);
                using (var session = _documentStore.OpenAsyncSession())
                {
                    try
                    {

                        await session.StoreAsync(ravenMessage);
                        _trace.TraceVerbose("Stored message with id '{0}'.", ravenMessage.Id);
                        if (_expiration > TimeSpan.Zero)
                        {
                            var expiry = DateTime.UtcNow.Add(_expiration);
                            session.Advanced.GetMetadataFor(ravenMessage)[RavenExpirationDate] = new RavenJValue(expiry);
                        }
                        await session.SaveChangesAsync();
                    }
                    catch (Exception e)
                    {
                        _trace.TraceError("Failed to store '{0}'", e.GetBaseException());
                    }
                }
                tcs.SetResult(null);
            }
            catch (Exception e)
            {
                tcs.SetUnwrappedException(e);
            }
        }

        internal void OnMessage(string id)
        {
            try
            {
                lock (_callbackLock)
                {
                    using (var session = _documentStore.OpenSession())
                    {
                        var message = session.Load<RavenMessage>(id);
                        this.OnReceived(message.StreamIndex, message.ToLongId(), message.ToScaleoutMessage());
                    }
                }
            }
            catch (Exception ex)
            {
                this.OnError(0, ex);
            }
        }

        private void ConnectWithRetry()
        {
            var connectTask = this.Connect();
            connectTask.ContinueWith(t =>
            {
                if (!t.IsFaulted)
                    return;

                _trace.TraceError("Error connecting to RavenDB - {0}", t.Exception);
                var oldState = Interlocked.Exchange(ref _state, _state);
                if (oldState == State.Disposing || oldState == State.Disposed)
                    return;

                Task.Delay(_reconnectDelay).ContinueWith(_ => this.ConnectWithRetry(), TaskContinuationOptions.OnlyOnRanToCompletion);
            }, TaskContinuationOptions.NotOnRanToCompletion);
        }

        private Task Connect()
        {
            this.Release();
            try
            {
                var documentStore = _documentStoreFactory();
                
                _trace.TraceInformation("Initializing connection '{0}' ...", documentStore.Identifier);

                documentStore.Initialize();

                _trace.TraceInformation("Connection initialized.");

                _documentStore = documentStore;

                _databaseChanges = _documentStore.Changes();
                _databaseChanges.ConnectionStatusChanged += this.OnDatabaseConnectionStatusChanged;

                return TaskAsyncHelper.Empty;
            }
            catch (Exception ex)
            {
                return TaskAsyncHelper.FromError(ex);
            }
        }

        private void OnDatabaseConnectionStatusChanged(object sender, EventArgs eventArgs)
        {
            var databaseChanges = (IDatabaseChanges)sender;

            var isConnected = databaseChanges.Connected;
            if (isConnected)
            {
                var oldState = Interlocked.Exchange(ref _state, State.Connected);
                if (oldState == State.Connected)
                    return;                

                _trace.TraceInformation("Connected to RavenDB, subscribe to events.");
                this.Subscribe(databaseChanges);
                this.Open(0);
            }
            else
            {
                var oldState = Interlocked.Exchange(ref _state, State.Closed);
                if (oldState == State.Closed)
                    return;

                this.Close(0);

                _trace.TraceInformation("Disonnected from RavenDB, unsubscribe to events.");
                this.Unsubscribe();

                if (oldState == State.Disposing || oldState == State.Disposed)
                    return;

                this.ConnectWithRetry();
            }
        }

        private void Subscribe(IDatabaseChanges changes)
        {
            this.Unsubscribe();

            var docIdPrefix = string.Format("{0}s/", typeof(RavenMessage).Name);
            _trace.TraceInformation("Subscribing to documents starting with '{0}'.", docIdPrefix);
            _subscription = changes.ForDocumentsStartingWith(docIdPrefix).Subscribe(_observer);
        }

        private void Unsubscribe()
        {
            var subscription = _subscription;
            _subscription = null;
            if (subscription == null)
                return;
            subscription.Dispose();
            _trace.TraceInformation("Unsubscripted from document changes.");
        }

        private void Shutdown()
        {
            _trace.TraceInformation("Shutdown ...");

            this.Release();

            _observer.Dispose();

            Interlocked.Exchange(ref _state, State.Disposed);

            _trace.TraceInformation("Goodbye.");
        }

        private void Release()
        {
            if (_subscription != null)
            {
                _subscription.Dispose();
                _subscription = null;
            }

            if (_databaseChanges != null)
            {
                _databaseChanges.ConnectionStatusChanged -= this.OnDatabaseConnectionStatusChanged;
                _databaseChanges = null;
            }

            if (_documentStore != null)
            {
                _documentStore.Dispose();
                _documentStore = null;
            }
        }

        internal void TraceInformation(string format, params object[] args)
        {
            _trace.TraceInformation(format, args);
        }

        internal void TraceError(string format, Exception ex)
        {
            _trace.TraceError(format);
        }

        internal void TraceVerbose(string fomrat, params object[] args)
        {
             _trace.TraceVerbose(fomrat, args);
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
