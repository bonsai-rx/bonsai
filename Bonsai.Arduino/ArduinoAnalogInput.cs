using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Arduino
{
    public class ArduinoAnalogInput : Source<int>
    {
        IObservable<int> analogInput;
        IDisposable connection;

        [TypeConverter(typeof(SerialPortNameConverter))]
        public string SerialPort { get; set; }

        public int Pin { get; set; }

        public override IDisposable Load()
        {
            analogInput = ObservableArduino.AnalogInput(SerialPort, Pin);
            return base.Load();
        }

        protected override void Start()
        {
            connection = analogInput.Subscribe(Subject);
        }

        protected override void Stop()
        {
            connection.Dispose();
        }
    }
}
