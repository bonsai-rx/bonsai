using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Arduino
{
    public class SysexReceivedEventArgs : EventArgs
    {
        public SysexReceivedEventArgs(byte feature, byte[] args)
        {
            Feature = feature;
            Args = args;
        }

        public byte Feature { get; private set; }

        public byte[] Args { get; private set; }
    }
}
