using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Arduino
{
    [DefaultProperty("Feature")]
    [Description("Sends a sequence of system exclusive messages to the specified Arduino.")]
    public class SendSysex : Sink<byte[]>
    {
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The feature ID used to identify the system exclusive message payload.")]
        public int Feature { get; set; }

        public override IObservable<byte[]> Process(IObservable<byte[]> source)
        {
            return Observable.Using(
                () => ArduinoManager.ReserveConnection(PortName),
                connection => source.Do(value =>
                {
                    lock (connection.Arduino)
                    {
                        connection.Arduino.SendSysex((byte)Feature, value);
                    }
                }));
        }
    }
}
