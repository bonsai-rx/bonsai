using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;

namespace Bonsai.IO
{
    [Description("Sinks the text representation of individual elements of the input sequence to a serial port.")]
    public class SerialStringWrite : Sink
    {
        [Description("The name of the serial port.")]
        [Editor("Bonsai.IO.Design.SerialPortConfigurationEditor, Bonsai.System.Design", typeof(UITypeEditor))]
        public string PortName { get; set; }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Using(
                () =>
                {
                    var writeLine = ObservableSerialPort.WriteLine(PortName);
                    var iterator = writeLine.GetEnumerator();
                    iterator.MoveNext();
                    return iterator;
                },
                iterator => source.Do(input => iterator.Current(input.ToString())));
        }
    }
}
