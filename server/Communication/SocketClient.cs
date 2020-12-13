using System;
using System.Net;
using System.Net.Sockets;

namespace Communication
{
    public class SocketClient
    {
        private readonly IPEndPoint _remoteEP;
        private readonly IPAddress _ip;
        public SocketClient(IPAddress ip, int port)
        {
            _ip = ip ?? throw new ArgumentNullException(nameof(ip));
            _remoteEP = new IPEndPoint(ip, port);
        }

        public void Send()
        {
            try
            {
                Socket socket = new Socket(_ip.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                IAsyncResult result = socket.BeginConnect(_remoteEP, null, null);

                // Connect using a timeout (3 seconds)
                bool success = result.AsyncWaitHandle.WaitOne(3000, true);

                if (socket.Connected)
                {
                    socket.EndConnect(result);
                }
                else
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                    socket.Dispose();
                }
            }
            catch (ArgumentNullException ane)
            {
                Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
            }
            catch (SocketException se)
            {
                throw new SocketException(se.ErrorCode);
            }
            catch (Exception e)
            {
                Console.WriteLine("Unexpected exception : {0}", e.ToString());
            }
        }
    }
}
