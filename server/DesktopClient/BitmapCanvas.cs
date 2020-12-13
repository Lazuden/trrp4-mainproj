using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace DesktopClient
{
    public class BitmapCanvas : Canvas
    {
        public const int BitmapWidth = 256;
        public const int BitmapHeight = 256;

        private readonly WriteableBitmap _bitmap;

        private bool _initialized;

        public BitmapCanvas()
        {
            _bitmap = new WriteableBitmap(BitmapWidth, BitmapHeight, 96.0, 96.0, PixelFormats.Rgb24, null);
        }

        protected override void OnRender(DrawingContext dc)
        {
            base.OnRender(dc);
            dc.PushTransform(new TranslateTransform(GetTranslateX(), GetTranslateY()));
            dc.DrawImage(_bitmap, new Rect(0.0, 0.0, _bitmap.PixelWidth, _bitmap.PixelHeight));
        }

        public int ToBitmapImageX(double x)
        {
            return (int)Math.Floor(x - GetTranslateX());
        }

        public int ToBitmapImageY(double y)
        {
            return (int)Math.Floor(y - GetTranslateY());
        }

        public void SetData(byte[] colorData)
        {
            if (_initialized)
            {
                //return;
            }
            _initialized = true;
            Int32Rect rect = new Int32Rect(0, 0, _bitmap.PixelWidth, _bitmap.PixelHeight);
            _bitmap.WritePixels(rect, colorData, 3 * _bitmap.PixelWidth, 0);
        }

        public void SetData(int x, int y, byte r, byte g, byte b)
        {
            if (x >= 0 && y >= 0 && x < _bitmap.PixelWidth && y < _bitmap.PixelHeight)
            {
                byte[] colorData = { r, g, b };
                Int32Rect rect = new Int32Rect(x, y, 1, 1);
                _bitmap.WritePixels(rect, colorData, 3, 0);
            }
        }

        private double GetTranslateX()
        {
            return -_bitmap.PixelWidth / 2.0;
        }

        private double GetTranslateY()
        {
            return -_bitmap.PixelHeight / 2.0;
        }
    }
}
