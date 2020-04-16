using System;

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
