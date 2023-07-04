using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that writes a sequence of bytes to a serial port.
    /// </summary>
    [DefaultProperty(nameof(PortName))]
    [Description("Writes a sequence of bytes to a serial port.")]
    public class SerialPortWrite : Sink<byte[]>
    {
        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        /// <summary>
        /// Writes an observable sequence of bytes to a serial port.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit unsigned integer arrays representing the binary data
        /// to write to the serial port.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the binary data to
        /// the serial port.
        /// </returns>
        public override IObservable<byte[]> Process(IObservable<byte[]> source)
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(PortName),
                connection => source.Do(value =>
                {
                    connection.SerialPort.Write(value, 0, value.Length);
                }));
        }

        /// <summary>
        /// Writes an observable sequence of byte array segments to a serial port.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit unsigned integer array segments representing the binary
        /// data to write to the serial port.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the binary data in each
        /// array segment to the serial port.
        /// </returns>
        public IObservable<ArraySegment<byte>> Process(IObservable<ArraySegment<byte>> source)
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(PortName),
                connection => source.Do(value =>
                {
                    connection.SerialPort.Write(value.Array, value.Offset, value.Count);
                }));
        }
    }
}
