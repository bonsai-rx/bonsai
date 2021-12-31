using System;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Provides data for the <see cref="Arduino.DigitalInputReceived"/> event.
    /// </summary>
    public class DigitalInputReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DigitalInputReceivedEventArgs"/>
        /// class using the port number and port pin state received in the digital input message.
        /// </summary>
        /// <param name="port">
        /// The number identifying the digital port (i.e. collection of 8 pins) from which
        /// the state transition event originated.
        /// </param>
        /// <param name="state">
        /// The state of all the digital input pins in the specified port at the time the
        /// transition occurred.
        /// </param>
        public DigitalInputReceivedEventArgs(int port, byte state)
        {
            Port = port;
            State = state;
        }

        /// <summary>
        /// Gets the number identifying the digital port from which the event originated.
        /// </summary>
        public int Port { get; private set; }

        /// <summary>
        /// Gets the state of all the digital input pins in the specified port at the time
        /// the transition occurred.
        /// </summary>
        public byte State { get; private set; }
    }
}
