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
    [DefaultProperty("Name")]
    [Description("Creates a connection to an Arduino board using the Firmata protocol.")]
    public class CreateArduino : Source<Arduino>, INamedElement
    {
        readonly ArduinoConfiguration configuration = new ArduinoConfiguration();

        [Description("The optional alias for the Arduino board.")]
        public string Name { get; set; }

        [TypeConverter(typeof(SerialPortNameConverter))]
        [Description("The name of the serial port connection.")]
        public string PortName
        {
            get { return configuration.PortName; }
            set { configuration.PortName = value; }
        }

        [TypeConverter(typeof(BaudRateConverter))]
        [Description("The baud rate used by the serial port connection.")]
        public int BaudRate
        {
            get { return configuration.BaudRate; }
            set { configuration.BaudRate = value; }
        }

        [Description("The interval (ms) controlling how often analog and I2C data are sampled and transmitted.")]
        public int SamplingInterval
        {
            get { return configuration.SamplingInterval; }
            set { configuration.SamplingInterval = value; }
        }

        public override IObservable<Arduino> Generate()
        {
            return Observable.Using(
                () => ArduinoManager.ReserveConnection(Name, configuration),
                resource =>
                {
                    return Observable.Return(resource.Arduino)
                                     .Concat(Observable.Never(resource.Arduino));
                });
        }
    }
}
