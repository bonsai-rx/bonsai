using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// Represents an operator that writes a sequence of binary or text data
    /// to a serial port.
    /// </summary>
    [DefaultProperty(nameof(PortName))]
    [Description("Writes a sequence of binary or text data to a serial port.")]
    public class SerialPortWrite : Sink<string>
    {
        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the separator used to terminate messages sent to the serial port.
        /// </summary>
        [Description("The separator used to terminate messages sent to the serial port.")]
        public string NewLine { get; set; } = ObservableSerialPort.DefaultNewLine;

        /// <summary>
        /// Writes an observable sequence of text messages to a serial port.
        /// </summary>
        /// <param name="source">
        /// The sequence containing the text messages to write to the serial port.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the text message data to
        /// the serial port, terminated by the specified line separator.
        /// </returns>
        public override IObservable<string> Process(IObservable<string> source)
        {
            var newLine = ObservableSerialPort.Unescape(NewLine);
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(PortName),
                connection => source.Do(value =>
                {
                    lock (connection.SerialPort)
                    {
                        connection.SerialPort.Write(value.ToString());
                        connection.SerialPort.Write(newLine);
                    }
                }));
        }

        /// <summary>
        /// Writes an observable sequence of binary messages to a serial port.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit unsigned integer arrays representing the binary messages
        /// to write to the serial port.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the binary message data to
        /// the serial port.
        /// </returns>
        public IObservable<byte[]> Process(IObservable<byte[]> source)
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(PortName),
                connection => source.Do(value =>
                {
                    lock (connection.SerialPort)
                    {
                        connection.SerialPort.Write(value, 0, value.Length);
                    }
                }));
        }

        /// <summary>
        /// Writes an observable sequence of binary messages represented as an array
        /// segment to a serial port.
        /// </summary>
        /// <param name="source">
        /// A sequence of 8-bit unsigned integer array segments representing the binary
        /// messages to write to the serial port.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect of writing the binary message
        /// data in each array segment to the serial port.
        /// </returns>
        public IObservable<ArraySegment<byte>> Process(IObservable<ArraySegment<byte>> source)
        {
            return Observable.Using(
                () => SerialPortManager.ReserveConnection(PortName),
                connection => source.Do(value =>
                {
                    lock (connection.SerialPort)
                    {
                        connection.SerialPort.Write(value.Array, value.Offset, value.Count);
                    }
                }));
        }
    }
}
