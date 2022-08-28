using System;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Provides data for the <see cref="Arduino.AnalogInputReceived"/> event.
    /// </summary>
    public class AnalogInputReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="AnalogInputReceivedEventArgs"/>
        /// class using the pin number and analog value received in the analog input message.
        /// </summary>
        /// <param name="pin">The pin number from which the analog value was sampled.</param>
        /// <param name="value">The digitized analog value.</param>
        public AnalogInputReceivedEventArgs(int pin, int value)
        {
            Pin = pin;
            Value = value;
        }

        /// <summary>
        /// Gets the pin number from which the analog value was sampled.
        /// </summary>
        public int Pin { get; private set; }

        /// <summary>
        /// Gets the digitized analog value.
        /// </summary>
        public int Value { get; private set; }
    }
}
