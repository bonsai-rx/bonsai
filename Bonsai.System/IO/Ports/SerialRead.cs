using System;
using System.ComponentModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.IO.Ports
{
    /// <summary>
    /// Represents an operator that reads a sequence of bytes from a serial port.
    /// </summary>
    [DefaultProperty(nameof(PortName))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Reads a sequence of bytes from a serial port.")]
    public class SerialRead : Source<byte[]>
    {
        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of bytes to read. Fewer bytes may be read
        /// if there are not enough bytes in the input buffer before the read timeout.
        /// </summary>
        [Description("The maximum number of bytes to read.")]
        public int Count { get; set; }

        /// <summary>
        /// Reads a single buffer of bytes from a serial port and surfaces
        /// the result through an observable sequence.
        /// </summary>
        /// <returns>
        /// A sequence containing a single array of 8-bit unsigned integers
        /// representing the binary data read from the serial port.
        /// </returns>
        public override IObservable<byte[]> Generate()
        {
            return ObservableSerialPort.Read(PortName, Count);
        }

        /// <summary>
        /// Reads a sequence of bytes from a serial port, where each new buffer
        /// is read only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for reading new buffers from
        /// the serial port.
        /// </param>
        /// <returns>
        /// A sequence of 8-bit unsigned integer arrays representing the binary data
        /// read from the serial port for each corresponding notification.
        /// </returns>
        public IObservable<byte[]> Generate<TSource>(IObservable<TSource> source)
        {
            var count = Count;
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(PortName),
                connection =>
                {
                    var serialPort = connection.SerialPort;
                    return source.SelectMany(async (_, cancellationToken) =>
                    {
                        var buffer = new byte[count];
                        await serialPort.BaseStream.ReadAsync(
                            buffer, 0, buffer.Length, cancellationToken);
                        return buffer;
                    });
                });
        }
    }
}
