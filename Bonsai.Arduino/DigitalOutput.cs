using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Drawing.Design;
using System.Reactive.Linq;

namespace Bonsai.Arduino
{
    [DefaultProperty("Pin")]
    [Description("Writes the sequence of digital state transitions to the specified Arduino output pin.")]
    public class DigitalOutput : Sink<bool>
    {
        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital output pin number for which to write values.")]
        public int Pin { get; set; }

        public override IObservable<bool> Process(IObservable<bool> source)
        {
            return Observable.Using(
                () =>
                {
                    var digitalOutput = ObservableArduino.DigitalOutput(PortName, Pin);
                    var iterator = digitalOutput.GetEnumerator();
                    iterator.MoveNext();
                    return iterator;
                },
                iterator => source.Do(input => iterator.Current(input)));
        }
    }
}
