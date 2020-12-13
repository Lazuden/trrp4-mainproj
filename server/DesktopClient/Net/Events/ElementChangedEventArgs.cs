namespace Pixels.Client.Net.Events
{
    public class ElementChangedEventArgs
    {
        public ElementChangedEventArgs(int x, int y, byte r, byte g, byte b)
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
