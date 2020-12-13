using Communication;
using DesktopClient.Tools;
using NetClient.Events;
using Pixels.Client.Net;
using Pixels.Client.Net.Events;
using Pixels.Client.Net.Messages;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DesktopClient
{
    public partial class MainWindow : Window, INotifyPropertyChanged, IMessageHandler
    {
        private Host _selectedHost;
        private readonly PixelsClient _client;
        private PixelsServerWrapper _server;
        private GameWindow _gameWindow;
        private Producer _producer;
        private Consumer _consumer;
        private CancellationTokenSource _cancellationTokenSource;


        public MainWindow()
        {
            InitializeComponent();
            _cancellationTokenSource = new CancellationTokenSource();
            Hosts = new ObservableCollection<Host>();
            ConnectCommand = new RelayCommand(Connect, () => !string.IsNullOrEmpty(Address));
            RefreshCommand = new RelayCommand(async () => await RefreshAsync());
            CreateHostCommand = new RelayCommand(CreateHost, () => !string.IsNullOrEmpty(HostName?.Trim()));
            SendMessageCommand = new RelayCommand(SendMessage, () => !string.IsNullOrEmpty(Message?.Trim()));
            DeleteHostCommand = new RelayCommand(DeleteHost);
            DataContext = this;

            _client = new PixelsClient();
            _client.ConnectionSucceeded += Client_ConnectionSucceeded;
            _client.ConnectionFailed += Client_ConnectionFailed;
            _client.Initialize += Client_Initialize;
            _client.ElementChanged += Client_ElementChanged;
            _client.ConnectionRefused += Client_ConnectionRefused;
            _client.Disconnected += Client_Disconnected;
            _client.ConnectionClosed += Client_ConnectionClosed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public ObservableCollection<Host> Hosts { get; private set; }

        public Host SelectedHost
        {
            get => _selectedHost;
            set
            {
                _selectedHost = value;
                Address = _selectedHost.Address;
            }
        }

        public RelayCommand ConnectCommand { get; private set; }
        public RelayCommand RefreshCommand { get; private set; }
        public RelayCommand CreateHostCommand { get; private set; }
        public RelayCommand SendMessageCommand { get; private set; }
        public RelayCommand DeleteHostCommand { get; private set; }

        public string HostName { get; set; }
        public string Address { get; set; }
        public string ChatText { get; set; }
        public string Message { get; set; }
        public bool IsHostsEnabled { get; set; }

        #region lol
        private void Client_ConnectionSucceeded(object sender, ConnectionSucceededEventArgs e)
        {
            Hide();

            _gameWindow = new GameWindow(Address);
            _gameWindow.ElementClicked += GameWindow_ElementClicked;
            _gameWindow.Closed += GameWindow_Closed;
            _gameWindow.Show();
        }

        private void Client_ConnectionFailed(object sender, ConnectionFailedEventArgs e)
        {
            MessageBox.Show("Не удалось подключиться к удаленному серверу");
        }

        private void Client_Initialize(object sender, InitializeEventArgs e)
        {
            _gameWindow.Dispatcher.Invoke(() => _gameWindow.SetBitmapData(e.Data));
        }

        private void Client_ElementChanged(object sender, ElementChangedEventArgs e)
        {
            _gameWindow.Dispatcher.Invoke((Action)(() => _gameWindow.SetBitmapData(e.X, e.Y, e.R, e.G, e.B)));
        }

        private void Client_ConnectionRefused(object sender, ConnectionRefusedEventArgs e)
        {
            MessageBox.Show("Сервер разорвал соединение");
            _gameWindow.Dispatcher.Invoke(() => _gameWindow.Close());
        }

        private void Client_Disconnected(object sender, DisconnectedEventArgs e)
        {
            _gameWindow.Dispatcher.Invoke(() =>
            {
                _gameWindow.Close();
                _server?.Stop();
                _server = null;
            });
        }

        private void Client_ConnectionClosed(object sender, ConnectionClosedEventArgs e)
        {
            if (_server == null)
            {
                MessageBox.Show("Сервер закрыл соединение");
            }

            _gameWindow.Dispatcher.Invoke(() =>
            {
                _gameWindow.Close();
                _server?.Stop();
                _server = null;
            });
        }

        private void GameWindow_ElementClicked(object sender, ElementClickedEventArgs e)
        {
            _client.SendMessage(new ChangeElementMessage(e.X, e.Y, e.R, e.G, e.B));
        }

        private void GameWindow_Closed(object sender, EventArgs e)
        {
            _client.Disconnect();
            Show();
        }
        #endregion

        private async void WindowLoaded(object sender, RoutedEventArgs e)
        {
            await RefreshAsync();
            var ip = new Config()["rabbitMQ_ip"];
            _producer = new Producer(ip);
            _consumer = new Consumer(ip);
            await Task.Run(() =>
            {
                _consumer.Run(this, _cancellationTokenSource.Token);
            });
        }
        
        private void Window_Closed(object sender, EventArgs e)
        {
            _cancellationTokenSource.Cancel();
        }

        private async Task RefreshAsync()
        {
            IsHostsEnabled = false;
            try
            {
                var hosts = await GrpcProvider.GetHostsAsync();
                Hosts = new ObservableCollection<Host>(hosts);
            }
            catch
            {
                MessageBox.Show("Не удалось обновить список серверов, так как сервер не отвечает");
            }
            finally
            {
                IsHostsEnabled = true;
            }
        }

        private void Connect()
        {
            if (IPEndPoint.TryParse(Address, out IPEndPoint ipEndPoint))
            {
                _client.Connect(ipEndPoint);
            }
            else
            {
                MessageBox.Show("Указанный адрес не является корректным");
            }
        }

        private async void CreateHost()
        {
            _server = new PixelsServerWrapper();
            _server.Start();

            string address = $"{_server.IPEndPoint.Address}:{_server.IPEndPoint.Port}";

            try
            {
                await GrpcProvider.SetHostAsync(HostName, address);
            }
            catch { }

            Address = address;

            _client.Connect(_server.IPEndPoint);
        }

        private bool _state;
        private async void SendMessage()
        {
            Message = Message.Trim();
            try
            {
                _producer.Send(Message);
                if (_state)
                {
                    if (!_consumer.IsRunning)
                    {
                        await Task.Run(() =>
                        {
                            _cancellationTokenSource.Cancel();
                            _cancellationTokenSource = new CancellationTokenSource();
                            _consumer.Run(this, _cancellationTokenSource.Token);
                        });
                    }
                    ChatText += "[Соединение с сервером чата восстановлено]" + Environment.NewLine;
                    _state = false;
                }
            }
            catch
            {
                ChatText += "[Соединение с сервером чата не установлено]" + Environment.NewLine;
                _state = true;
            }
            if (!_consumer.IsRunning)
            {
                await Task.Run(() =>
                {
                    _cancellationTokenSource.Cancel();
                    _cancellationTokenSource = new CancellationTokenSource();
                    _consumer.Run(this, _cancellationTokenSource.Token);
                });
            }

            Message = string.Empty;
        }

        public void Handle(string message)
        {
            ChatText += "• " + message + Environment.NewLine;
        }

        private void TextBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                if (SendMessageCommand.CanExecute(null))
                {
                    SendMessageCommand.Execute(null);
                }
            }
        }

        private void DeleteHost()
        {
            try
            {
                if (File.Exists("world_data.bin"))
                    File.Delete("world_data.bin");
            }
            catch { Debug.WriteLine("удаление чот хромает"); }
        }
    }
}
