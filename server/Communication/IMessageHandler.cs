using System;
using System.Collections.Generic;
using System.Text;

namespace Communication
{
    public interface IMessageHandler
    {
        void Handle(string message);
    }
}
