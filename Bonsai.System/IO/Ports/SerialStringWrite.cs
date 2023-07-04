using System;
using System.ComponentModel;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that writes the text representation of each element of the
    /// sequence to a serial port.
    /// </summary>
    [DefaultProperty(nameof(PortName))]
    [Description("Writes the text representation of each element of the sequence to a serial port.")]
    public class SerialStringWrite : Sink
    {
        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the separator used to terminate lines sent to the serial port.
        /// </summary>
        [Description("The separator used to terminate lines sent to the serial port.")]
        public string NewLine { get; set; }

        /// <summary>
        /// Writes the text representation of each element of an observable sequence to a serial port.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the elements to write to the serial port.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the elements to
        /// the serial port.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var newLine = SerialPortManager.Unescape(NewLine);
            return ObservableSerialPort.WriteLine(source, PortName, newLine);
        }
    }
}
