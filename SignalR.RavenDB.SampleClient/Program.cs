using System;
using System.Threading;
using System.Threading.Tasks;
using SignalR.RavenDB.SampleCommon;

namespace SignalR.RavenDB.SampleClient
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var clients = new Client[Consts.Instances*2];
            var count = 0;
            for (var i = 0; i < Consts.Instances; ++i)
            {
                for (var j = 0; j < Consts.Instances; ++j)
                {
                    clients[count] = new Client(string.Format("Client({0})", count), Consts.Port + i);
                    var task = clients[count].Start();

                    if (count == 3) // only last client sends
                    {
                        task.ContinueWith(
                            (_, client) => OnClientConnected((Client)client),
                            clients[count], cts.Token,
                            TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);
                    }
                    count++;
                }                
            }

            Console.ReadLine();
            cts.Cancel();
        }

        private static void OnClientConnected(Client client)
        {
            Task.Run(async () =>
            {
                var count = 1;
                while (true)
                {
                    client.Send(string.Format("This is msg #{0}", count++));
                    await Task.Delay(TimeSpan.FromSeconds(1d));
                }
            });
        }
    }
}
