using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Communication
{
    public static class GrpcProvider
    {
        private static readonly string _uri = "http://" + new Config()["server_uri"];

        public static async Task<List<Host>> GetHostsAsync()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

            using var channel = GrpcChannel.ForAddress(_uri);
            var client = new HostsManager.HostsManagerClient(channel);

            var response = await client.GetHostsAsync(new EmptyParams());
            var hosts = response.Hosts_.ToList();

            await Task.Run(() =>
            {
                hosts.AsParallel().ForAll(host =>
                {
                    var ping = PingHost(host.Address) ?? 999;
                    host.Ping = ping;
                });
            });

            return hosts;
        }

        private static int? PingHost(string nameOrAddress)
        {
            try
            {
                bool pingable = false;
                Ping pinger = null;
                PingReply reply = null;
                var ipAddress = IPEndPoint.Parse(nameOrAddress);
                try
                {
                    pinger = new Ping();
                    reply = pinger.Send(ipAddress.Address, 1000);
                    pingable = reply.Status == IPStatus.Success;
                }
                catch (PingException ex)
                {
                    // Discard PingExceptions and return false;
                }
                finally
                {
                    if (pinger != null)
                    {
                        pinger.Dispose();
                    }
                }

                if (pingable)
                {

                    var tcpClient = new TcpClient();
                    try
                    {
                        tcpClient.Connect(ipAddress);
                    }
                    catch (SocketException)
                    {
                        return null;
                    }
                    finally
                    {
                        tcpClient.Close();
                    }
                    return (int)reply.RoundtripTime;
                }
            }
            catch
            {
                return null;
            }
            return null;
        }

        public static async Task SetHostAsync(string hostName, string address)
        {
            using var channel = GrpcChannel.ForAddress(_uri);
            var client = new HostsManager.HostsManagerClient(channel);
            var response = await client.SetHostAsync(new Host { Address = address, Name = hostName });
        }
    }
}
