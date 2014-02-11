using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR;

namespace SignalR.RavenDB.SampleServer
{
    public class ChatHub : Hub
    {
        public override Task OnConnected()
        {
            Console.WriteLine("Client with id {0} just connected.", this.Context.ConnectionId);

            this.Clients.Caller.OnReceived("Server: Live long and prosper.");
            return base.OnConnected();
        }

        public override Task OnDisconnected()
        {
            Console.WriteLine("Client with id {0} just disconnected.", this.Context.ConnectionId);
            return base.OnDisconnected();
        }

        public void Send(string name, string message)
        {
            this.Clients.Others.OnReceived(string.Format("{0}: {1}", name, message));
        }
    }
}