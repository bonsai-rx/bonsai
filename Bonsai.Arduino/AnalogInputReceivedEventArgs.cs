using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Arduino
{
    public class AnalogInputReceivedEventArgs : EventArgs
    {
        public AnalogInputReceivedEventArgs(int pin, int value)
        {
            Pin = pin;
            Value = value;
        }

        public int Pin { get; private set; }

        public int Value { get; private set; }
    }
}
