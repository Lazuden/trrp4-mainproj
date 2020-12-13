using Communication;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;

namespace Pixels.Client.Net
{
    public sealed class PixelsServerWrapper
    {
        public readonly string _javaExecutor;
        public readonly string _serverJarFileName;

        private Process _process;

        public PixelsServerWrapper()
        {
            var config = new Config();
            _javaExecutor = config["javaw"];
            _serverJarFileName = config["pixels-server-1.0-SNAPSHOT-jar-with-dependencies.jar"];
        }

        public IPEndPoint IPEndPoint { get; set; }

        public bool Running { get => _process != null; }

        public void Start()
        {
            if (!Running)
            {
                ProcessStartInfo info = new ProcessStartInfo
                {
                    FileName = _javaExecutor,
                    Arguments = $"-jar {_serverJarFileName}",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardInput = true
                };
                _process = Process.Start(info);
                IPEndPoint = ReadAddress();
            }
            else
            {
                throw new InvalidOperationException("server is already started");
            }
        }

        private IPEndPoint ReadAddress()
        {
            StreamReader reader = _process.StandardOutput;
            string address = reader.ReadLine();
            return IPEndPoint.Parse(address);
        }

        public void Stop()
        {
            if (Running)
            {
                StreamWriter writer = _process.StandardInput;
                writer.Write('\n');
                _process.WaitForExit();
                _process = null;
            }
            else
            {
                throw new InvalidOperationException("server is not started");
            }
        }
    }
}
