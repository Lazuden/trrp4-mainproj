using NetClient;

namespace Pixels.Client.Net.Messages
{
    public class ChangeElementMessage : IMessage
    {
        public ChangeElementMessage(int x, int y, byte r, byte g, byte b)
        {
            X = x;
            Y = y;
            R = r;
            G = g;
            B = b;
        }
            
        public byte Id => 0x00;

        public int X { get; }

        public int Y { get; }

        public byte R { get; }

        public byte G { get; }
        
        public byte B { get; }

        public byte[] Serialize()
        {
            return new byte[] { (byte)X, (byte)Y, R, G, B };
        }
    }
}
