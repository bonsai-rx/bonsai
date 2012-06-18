using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.Ports;
using System.ComponentModel;

namespace Bonsai.IO
{
    public class SerialStringRead : Source<string>
    {
        IObservable<string> readLine;
        IDisposable connection;

        [TypeConverter(typeof(SerialPortNameConverter))]
        public string SerialPort { get; set; }

        public override IDisposable Load()
        {
            readLine = ObservableSerialPort.ReadLine(SerialPort);
            return base.Load();
        }

        protected override void Unload()
        {
            readLine = null;
            base.Unload();
        }

        protected override void Start()
        {
            connection = readLine.Subscribe(Subject);
        }

        protected override void Stop()
        {
            connection.Dispose();
        }
    }
}
