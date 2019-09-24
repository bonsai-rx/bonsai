using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    [DefaultProperty("PortName")]
    [Description("Sinks the text representation of individual elements of the input sequence to a serial port.")]
    public class SerialStringWrite : Sink
    {
        public SerialStringWrite()
        {
            NewLine = ObservableSerialPort.DefaultNewLine;
        }

        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port.")]
        public string PortName { get; set; }

        [Description("The value used to terminate lines sent to the serial port.")]
        public string NewLine { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
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
    }
}
