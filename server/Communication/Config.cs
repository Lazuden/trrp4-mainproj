using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Communication
{
    public class Config
    {
        Dictionary<string, string> _dictionary;

        public Config(string filename = "config.txt")
        {
            _dictionary = new Dictionary<string, string>();
            try
            {
                using var reader = new StreamReader(filename);
                while (!reader.EndOfStream)
                {
                    var slices = reader.ReadLine().Split("=");
                    _dictionary.Add(slices[0], slices[1]);
                }
            }
            catch { }
        }

        public string this[string ds]
        {
            get
            {
                if (_dictionary.ContainsKey(ds))
                {
                    return _dictionary[ds];
                }
                return null;
            }
        }
    }
}
