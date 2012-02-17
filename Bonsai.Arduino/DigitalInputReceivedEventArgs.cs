using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Arduino
{
    public class DigitalInputReceivedEventArgs : EventArgs
    {
        public DigitalInputReceivedEventArgs(int port, int state)
        {
            Port = port;
            State = state;
        }

        public int Port { get; private set; }

        public int State { get; private set; }
    }
}
