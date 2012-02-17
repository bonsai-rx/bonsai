using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Arduino
{
    public class ArduinoDigitalInput : Source<bool>
    {
        IObservable<bool> digitalInput;
        IDisposable connection;

        [TypeConverter(typeof(SerialPortNameConverter))]
        public string SerialPort { get; set; }

        public int Pin { get; set; }

        public override IDisposable Load()
        {
            digitalInput = ObservableArduino.DigitalInput(SerialPort, Pin);
            return base.Load();
        }

        protected override void Unload()
        {
            digitalInput = null;
            base.Unload();
        }

        protected override void Start()
        {
            connection = digitalInput.Subscribe(Subject);
        }

        protected override void Stop()
        {
            connection.Dispose();
        }
    }
}
