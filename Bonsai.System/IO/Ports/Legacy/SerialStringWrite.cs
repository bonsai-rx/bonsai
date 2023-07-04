using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Ports.SerialWriteLine"/> operator instead.
    /// </summary>
    [Obsolete]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Description("This type is obsolete. Please use the Ports.SerialWriteLine operator instead.")]
    public class SerialStringWrite : Sink
    {
        /// <summary>
        /// Gets or sets the name of the serial port.
        /// </summary>
        [TypeConverter(typeof(Ports.PortNameConverter))]
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
            var newLine = Ports.SerialPortManager.Unescape(NewLine);
            return Observable.Using(
                () => Ports.SerialPortManager.ReserveConnection(PortName),
                connection =>
                {
                    return source.Do(value =>
                    {
                        lock (connection.SerialPort)
                        {
                            connection.SerialPort.Write(value.ToString());
                            connection.SerialPort.Write(newLine);
                        }
                    });
                });
        }
    }
}
