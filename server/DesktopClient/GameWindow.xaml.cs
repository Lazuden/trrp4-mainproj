using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace DesktopClient
{
    public partial class GameWindow : Window
    {
        private SolidColorBrush _selectedBrush;

        public GameWindow(string address)
        {
            InitializeComponent();
            DataContext = this;

            Address = address;

            _selectedBrush = Brushes.Black;

            SolidColorBrush[] brushes = {
                /*lol*/
                new SolidColorBrush(Color.FromRgb(2, 2, 2)), // black
                new SolidColorBrush(Color.FromRgb(255, 255, 255)), // white
                new SolidColorBrush(Color.FromRgb(255, 2, 2)), // red
                new SolidColorBrush(Color.FromRgb(2, 255, 2)), // green
                new SolidColorBrush(Color.FromRgb(2, 2, 255)), // blue
                new SolidColorBrush(Color.FromRgb(255, 255, 2)), // yellow
                new SolidColorBrush(Color.FromRgb(255, 2, 255)), // magenta
                new SolidColorBrush(Color.FromRgb(2, 255, 255)), // cyan
                /*Brushes.Black,
                Brushes.White,
                Brushes.Red,
                Brushes.Green,
                Brushes.Blue,
                Brushes.Yellow,
                Brushes.Magenta,
                Brushes.Cyan*/
            };
            Array.ForEach(brushes, CreateColorButton);

            _zoomControl.Loaded += (sender, e) =>
            {
                try
                {
                    _zoomControl.ZoomToOriginal();
                }
                catch { /* ignored*/ }
            };
        }

        public delegate void ElementClickedEventHandler(object sender, ElementClickedEventArgs e);

        public event ElementClickedEventHandler ElementClicked;

        public string Address { get; set; }

        private void AddressButton_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(Address);
        }

        public void ZoomControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        public void BitmapCanvas_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            ProcessMouseEvent(e);
        }

        public void BitmapCanvas_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                ProcessMouseEvent(e);
            }
        }

        private void ProcessMouseEvent(MouseEventArgs e)
        {
            int x = _bitmapCanvas.ToBitmapImageX(e.GetPosition(_bitmapCanvas).X);
            int y = _bitmapCanvas.ToBitmapImageY(e.GetPosition(_bitmapCanvas).Y);
            byte r = _selectedBrush.Color.R;
            byte g = _selectedBrush.Color.G;
            byte b = _selectedBrush.Color.B;
            ElementClicked?.Invoke(this, new ElementClickedEventArgs(x, y, r, g, b));
        }

        private void CreateColorButton(SolidColorBrush brush)
        {
            Button button = new Button { Width = 32, Height = 32, Background = brush };
            button.Click += (sender, args) => _selectedBrush = brush;
            _toolBar.Items.Add(button);
        }

        public void SetBitmapData(byte[] colorData)
        {
            _bitmapCanvas.SetData(colorData);
        }

        public void SetBitmapData(int x, int y, byte r, byte g, byte b)
        {
            _bitmapCanvas.SetData(x, y, r, g, b);
        }
    }

    public class ElementClickedEventArgs
    {
        public ElementClickedEventArgs(int x, int y, byte r, byte g, byte b)
        {
            X = x;
            Y = y;
            R = r;
            G = g;
            B = b;
        }

        public int X { get; }

        public int Y { get; }

        public byte R { get; }

        public byte G { get; }

        public byte B { get; }
    }
}
