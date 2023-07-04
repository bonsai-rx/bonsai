using System;
using System.ComponentModel;

namespace Bonsai.IO
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Ports.SerialReadLine"/> operator instead.
    /// </summary>
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("This type is obsolete. Please use the Ports.SerialReadLine operator instead.")]
    public class SerialStringRead : Source<string>
    {
        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(Ports.PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the new line separator used to delimit reads from the serial port.
        /// </summary>
        [Description("The new line separator used to delimit reads from the serial port.")]
        public string NewLine { get; set; }

        /// <summary>
        /// Reads a sequence of characters delimited by a new line separator from the serial port.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing each of the lines
        /// read from the serial port.
        /// </returns>
        public override IObservable<string> Generate()
        {
            var newLine = Ports.SerialPortManager.Unescape(NewLine);
            return Ports.ObservableSerialPort.ReadLine(PortName, newLine);
        }
    }
}
