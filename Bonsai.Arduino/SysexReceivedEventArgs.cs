using System;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Provides data for the <see cref="Arduino.SysexReceived"/> event.
    /// </summary>
    public class SysexReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SysexReceivedEventArgs"/>
        /// class using the specified feature ID and optional data payload.
        /// </summary>
        /// <param name="feature">
        /// The identifier of the system exclusive (SysEx) feature received in
        /// the message event.
        /// </param>
        /// <param name="args">
        /// The data payload received together with the SysEx message.
        /// </param>
        public SysexReceivedEventArgs(byte feature, byte[] args)
        {
            Feature = feature;
            Args = args;
        }

        /// <summary>
        /// Gets the identifier of the system exclusive (SysEx) feature received in
        /// the message event.
        /// </summary>
        public byte Feature { get; private set; }

        /// <summary>
        /// Gets the data payload received together with the SysEx message.
        /// </summary>
        public byte[] Args { get; private set; }
    }
}
