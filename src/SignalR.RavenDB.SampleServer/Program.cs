using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Owin.Hosting;
using SignalR.RavenDB.SampleCommon;

namespace SignalR.RavenDB.SampleServer
{
    class Program
    {
        static void Main(string[] args)
        {
            var cts = new CancellationTokenSource();
            var tasks = new Task[Consts.Instances];
            for (var i = 0; i < Consts.Instances; i++)
            {
                tasks[i] = StartServerTask(Consts.Port + i, cts.Token);
            }

            Console.ReadLine();
            cts.Cancel();

            Task.WaitAll(tasks);
        }

        private static Task StartServerTask(int port, CancellationToken cancellationToken)
        {
            var url = string.Format("http://*:{0}", port);
            return Task.Run(() =>
            {
                try
                {
                    using (WebApp.Start<Startup>(url))
                    {
                        Console.WriteLine("Server running on '{0}'", url);
                        cancellationToken.WaitHandle.WaitOne();
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine("Failed to start server on '{0}': {1}", url, ex.GetBaseException());
                }
                
            }, cancellationToken);
        }
    }
}
