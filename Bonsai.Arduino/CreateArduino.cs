using Bonsai.IO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    [Description("Creates a connection to an Arduino board using the Firmata protocol.")]
    public class CreateArduino : Source<Arduino>
    {
        public CreateArduino()
        {
            BaudRate = Arduino.DefaultBaudRate;
            SamplingInterval = Arduino.DefaultSamplingInterval;
        }

        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port connection.")]
        public string PortName { get; set; }

        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The baud rate used by the serial port connection.")]
        public int BaudRate { get; set; }

        [Description("The interval (ms) controlling how often analog and I2C data are sampled and transmitted.")]
        public int SamplingInterval { get; set; }

        public override IObservable<Arduino> Generate()
        {
            return Observable.Using(
                () => ArduinoManager.ReserveConnection(PortName, BaudRate, SamplingInterval),
                resource =>
                {
                    return Observable.Return(resource.Arduino)
                                     .Concat(Observable.Never(resource.Arduino));
                });
        }
    }
}
