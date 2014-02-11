using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Transports;

namespace SignalR.RavenDB.SampleClient
{
    public class Client
    {
        private readonly HubConnection _hubConnection;
        private readonly IHubProxy _chatProxy;
        private readonly string _name;

        public Client(string name, int port)
        {
            _name = name;
            _hubConnection = new HubConnection(string.Format("http://127.0.0.1:{0}", port));
            _chatProxy = _hubConnection.CreateHubProxy("ChatHub");
            _chatProxy.On<string>("OnReceived", this.OnReceived);            
        }

        public Task Start()
        {
            return _hubConnection.Start(new LongPollingTransport());
        }

        public void Send(string message)
        {
            _chatProxy.Invoke("Send", _name, message);
        }

        private void OnReceived(string message)
        {
            Console.WriteLine("{0} received => {1}", _name, message);
        }        
    }
}
