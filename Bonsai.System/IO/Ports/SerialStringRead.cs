using System;
using System.ComponentModel;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that reads lines of characters asynchronously from a serial port.
    /// </summary>
    [DefaultProperty(nameof(PortName))]
    [Description("Reads lines of characters asynchronously from a serial port.")]
    public class SerialStringRead : Source<string>
    {
        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the new line separator used to delimit reads from the serial port.
        /// </summary>
        [Description("The new line separator used to delimit reads from the serial port.")]
        public string NewLine { get; set; }

        /// <summary>
        /// Reads lines of characters asynchronously from the serial port.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing each of the lines
        /// read from the serial port.
        /// </returns>
        public override IObservable<string> Generate()
        {
            var newLine = SerialPortManager.Unescape(NewLine);
            return ObservableSerialPort.ReadLine(PortName, newLine);
        }
    }
}
