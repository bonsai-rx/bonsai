using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Arduino
{
    public class SysexReceivedEventArgs : EventArgs
    {
        public SysexReceivedEventArgs(byte command, byte[] args)
        {
            Command = command;
            Args = args;
        }

        public byte Command { get; private set; }

        public byte[] Args { get; private set; }
    }
}
