using Communication;
using Grpc.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Server.Services
{
    public class HostWrapper
    {
        public HostWrapper(Host host)
        {
            Host = host ?? throw new ArgumentNullException(nameof(host));
            Count = 2;
        }

        public Host Host { get; set; }
        public int Count { get; set; } //осталось попыток подключения
    }

    public class HostsManagerService : HostsManager.HostsManagerBase
    {
        private List<HostWrapper> _hosts;
        private DbProvider _dbProvider;

        public HostsManagerService(CancellationToken cancellationToken)
        {
            _dbProvider = new DbProvider(new Config()["dbPath"]);
            _hosts = _dbProvider.GetHosts().Select(x => new HostWrapper(x)).ToList();

            var refreshHostsTask = Task.Run(() =>
            {
                var cancelWaitTask = Task.Run(() =>
                {
                    using var resetEvent = new ManualResetEvent(false);
                    cancellationToken.Register(() => resetEvent.Set());
                    resetEvent.WaitOne();
                });

                while (!cancellationToken.IsCancellationRequested)
                {
                    Console.WriteLine($"запуск сборщика мусора для хостов на сервере. сейчас {_hosts.Count} хостов {DateTime.Now}");
                    var refrashedHosts = _hosts.AsParallel().Where(x => IsHostAlive(x) || x.Count > 0).ToList();
                    Console.WriteLine($"стоп для сборщика мусора для хостов на сервере. сейчас {refrashedHosts.Count} хостов {DateTime.Now}");
                    _hosts = refrashedHosts;

                    _dbProvider.SaveHosts(_hosts.Select(x => x.Host).ToList());

                    var delayTask = Task.Delay(10_000);
                    Task.WaitAny(cancelWaitTask, delayTask);
                    if (cancelWaitTask.IsCompleted)
                    {
                        break;
                    }
                }
            });
        }

        public override Task<Hosts> GetHosts(EmptyParams request, ServerCallContext context)
        {
            var result = new Hosts();
            result.Hosts_.Add(_hosts.Select(x => x.Host));
            return Task.FromResult(result);
        }

        public override Task<SetHostResponse> SetHost(Host request, ServerCallContext context)
        {
            if (_hosts.FirstOrDefault(x => x.Host.Address == request.Address) is null)
            {
                _hosts.Add(new HostWrapper(request));
            }
            return Task.FromResult(new SetHostResponse());
        }

        private bool IsHostAlive(HostWrapper hostWrapper)
        {
            var ipEndPoint = IPEndPoint.Parse(hostWrapper.Host.Address);
            var tcpClient = new TcpClient();
            try
            {
                tcpClient.Connect(ipEndPoint);
                hostWrapper.Count = 2;
                return true;
            }
            catch (SocketException)
            {
                hostWrapper.Count--;
            }
            finally
            {
                tcpClient.Close();
            }
            return false;
        }
    }
}
