using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.IO
{
    public class SerialStringWrite : Sink<string>
    {
        IEnumerable<Action<string>> analogOutput;
        IEnumerator<Action<string>> iterator;

        [TypeConverter(typeof(SerialPortNameConverter))]
        public string SerialPort { get; set; }

        public override void Process(string input)
        {
            iterator.Current(input);
        }

        public override IDisposable Load()
        {
            analogOutput = ObservableSerialPort.WriteLine(SerialPort);
            iterator = analogOutput.GetEnumerator();
            iterator.MoveNext();
            return base.Load();
        }

        protected override void Unload()
        {
            iterator.Dispose();
            analogOutput = null;
            base.Unload();
        }
    }
}
