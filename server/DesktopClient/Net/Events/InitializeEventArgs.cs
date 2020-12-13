using System;

namespace Pixels.Client.Net.Events
{
    public class InitializeEventArgs
    {
        private readonly byte[] _data;

        public InitializeEventArgs(byte[] data)
        {
            _data = data;
        }

        public byte[] Data
        {
            get
            {
                return _data.Clone() as byte[];
            }
        }
    }
}
