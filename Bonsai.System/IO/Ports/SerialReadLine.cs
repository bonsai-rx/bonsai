using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.IO.Ports
{
    /// <summary>
    /// Represents an operator that reads lines of characters asynchronously from a serial port.
    /// </summary>
    [DefaultProperty(nameof(PortName))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Reads lines of characters asynchronously from a serial port.")]
    public class SerialReadLine : Source<string>
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
        /// Reads a sequence of characters delimited by a new line separator from the serial port.
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

        /// <summary>
        /// Reads a sequence of lines from a serial port, where each new line
        /// is read only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for reading new lines from
        /// the serial port.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="string"/> values representing each of the lines
        /// read from the serial port for each corresponding notification.
        /// </returns>
        public IObservable<string> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(PortName),
                connection => source.Select(_ => connection.SerialPort.ReadLine()));
        }
    }
}
