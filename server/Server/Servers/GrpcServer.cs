using Grpc.Core;
using Server.Services;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Servers
{
    public class GrpcServer : IServer
    {
        private readonly Grpc.Core.Server _server;
        private readonly string _host;
        private readonly int _port;

        public GrpcServer(string host, int port, CancellationToken cancellationToken)
        {
            _server = new Grpc.Core.Server
            {
                Services = { HostsManager.BindService(new HostsManagerService(cancellationToken)) },
                Ports = { new ServerPort(host, port, ServerCredentials.Insecure) }
            };
            _host = host;
            _port = port;
        }

        public void Run(CancellationToken cancellationToken)
        {
            Console.WriteLine($"SERVER is running on {_host}:{_port}");
            _server.Start();

            using var resetEvent = new ManualResetEvent(false);
            cancellationToken.Register(() => resetEvent.Set());
            resetEvent.WaitOne();

            Console.WriteLine($"SERVER is closing ({_host}:{_port})");
            var task = Task.Run(async () => await _server.ShutdownAsync());
            Task.WaitAll(task);
        }
    }
}
