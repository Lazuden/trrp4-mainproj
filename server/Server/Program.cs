using Communication;
using Server.Servers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Server
{
    public class Program
    {
        private readonly CancellationTokenSource _cancellationTokenSource;
        public Program(string serverUri)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            var endPoint = IPEndPoint.Parse(serverUri);
            _ip = endPoint.Address.ToString();
            _port = endPoint.Port;
        }

        private readonly string _ip;
        private readonly int _port;

        public static void Main(string[] args)
        {
            var serverUri = new Config()["server_uri"];

            var program = new Program(serverUri);
            Console.CancelKeyPress += (sender, e) =>
            {
                try
                {
                    program.Cancel();
                }
                finally
                {
                    e.Cancel = true;
                }
            };
            program.Run();
        }

        private void Run()
        {
            string host = _ip;
            int port = _port;

            var servers = new List<IServer>()
            {
                new GrpcServer(host, port, _cancellationTokenSource.Token), // собсна сам сервак
            };

            var tasks = servers.Select(x => Task.Run(() => x.Run(_cancellationTokenSource.Token))).ToArray();

            Task.WaitAll(tasks);
            Console.WriteLine("All servers are closed. Press any key");
            Console.ReadKey();
        }

        private void Cancel()
        {
            _cancellationTokenSource.Cancel();
        }
    }
}
