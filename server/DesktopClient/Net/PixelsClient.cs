using NetClient;
using NetClient.Events;
using Pixels.Client.Net.Events;
using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace Pixels.Client.Net
{
    public sealed class PixelsClient
    {
        /* List of IDs of incoming messages. */
        private const int InitializeMessageId = 0x00;
        private const int ElementChangedMessageId = 0x01;
        /* List of lengths of incoming messages. */
        private const int InitializeMessageLength = 256 * 256 * 3;
        private const int ElementChangedMessageLength = 5;

        private readonly NetClientHandler _handler;

        public delegate void ConnectionFailedEventHandler(object sender, ConnectionFailedEventArgs e);
        public delegate void ConnectionSucceededEventHandler(object sender, ConnectionSucceededEventArgs e);
        public delegate void InitializeEventHandler(object sender, InitializeEventArgs e);
        public delegate void ElementChangedEventHandler(object sender, ElementChangedEventArgs e);
        public delegate void ConnectionClosedEventHandler(object sender, ConnectionClosedEventArgs e);
        public delegate void DisconnectedEventHandler(object sender, DisconnectedEventArgs e);
        public delegate void ConnectionRefusedEventHandler(object sender, ConnectionRefusedEventArgs e);

        public event ConnectionFailedEventHandler ConnectionFailed;
        public event ConnectionSucceededEventHandler ConnectionSucceeded;
        public event InitializeEventHandler Initialize;
        public event ElementChangedEventHandler ElementChanged;
        public event ConnectionClosedEventHandler ConnectionClosed;
        public event DisconnectedEventHandler Disconnected;
        public event ConnectionRefusedEventHandler ConnectionRefused;

        public PixelsClient()
        {
            _handler = new NetClientHandler();
            _handler.ConnectionFailed += InvokeConnectionFailed;
            _handler.ConnectionSucceeded += InvokeConnectionSucceeded;
            _handler.MessageReceived += InvokeMessageReceived;
            _handler.Disconnected += InvokeDisconnected;
            _handler.ConnectionClosed += InvokeConnectionClosed;
            _handler.ConnectionRefused += InvokeConnectionRefused;
        }

        private void InvokeConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            ConnectionFailed?.Invoke(sender, e);
        }

        private void InvokeConnectionSucceeded(object sender, ConnectionSucceededEventArgs e)
        {
            ConnectionSucceeded?.Invoke(sender, e);
        }

        private void InvokeMessageReceived(object sender, MessageReceivedEventArgs e)
        {
            switch (e.MessageId)
            {
                case InitializeMessageId:
                    ProcessInitializeMessage(e.Stream);
                    break;
                case ElementChangedMessageId:
                    ProcessElementChangedMessage(e.Stream);
                    break;
            }
        }

        private void ProcessInitializeMessage(NetworkStream stream)
        {
            /*byte[] bytes = new byte[InitializeMessageLength];
            stream.Read(bytes, 0, bytes.Length);*/

            byte[] bytes = stream.ReadNBytes(InitializeMessageLength);
            Initialize?.Invoke(this, new InitializeEventArgs(data: bytes));
        }

        private void ProcessElementChangedMessage(NetworkStream stream)
        {
            /*byte[] bytes = new byte[ElementChangedMessageLength];
            stream.Read(bytes, 0, bytes.Length);*/

            byte[] bytes = stream.ReadNBytes(ElementChangedMessageLength);
            ElementChanged?.Invoke(this, new ElementChangedEventArgs(x: bytes[0], y: bytes[1], r: bytes[2], g: bytes[3], b: bytes[4]));
        }

        private void InvokeConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            ConnectionClosed?.Invoke(sender, e);
        }

        private void InvokeDisconnected(object sender, DisconnectedEventArgs e)
        {
            Disconnected?.Invoke(sender, e);
        }

        private void InvokeConnectionRefused(object sender, ConnectionRefusedEventArgs e)
        {
            ConnectionRefused?.Invoke(sender, e);
        }

        public void Connect(IPEndPoint ipEndPoint)
        {
            _handler.Connect(ipEndPoint);
        }

        public void Disconnect()
        {
            _handler.Disconnect();
        }

        public void SendMessage(IMessage message)
        {
            _handler.SendMessage(message);
        }
    }
}
