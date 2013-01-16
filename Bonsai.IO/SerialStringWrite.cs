using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.IO
{
    public class SerialStringWrite : Sink<string>
    {
        IEnumerable<Action<string>> writeLine;
        IEnumerator<Action<string>> iterator;

        [Editor("Bonsai.IO.Design.SerialPortConfigurationEditor, Bonsai.IO.Design", typeof(UITypeEditor))]
        public string SerialPort { get; set; }

        public override void Process(string input)
        {
            iterator.Current(input);
        }

        public override IDisposable Load()
        {
            writeLine = ObservableSerialPort.WriteLine(SerialPort);
            iterator = writeLine.GetEnumerator();
            iterator.MoveNext();
            return base.Load();
        }

        protected override void Unload()
        {
            iterator.Dispose();
            writeLine = null;
            base.Unload();
        }
    }
}
