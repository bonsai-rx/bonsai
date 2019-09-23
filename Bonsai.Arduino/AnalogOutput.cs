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
    [Description("Writes the sequence of numerical values to the specified Arduino output pin using PWM.")]
    public class AnalogOutput : Sink<int>
    {
        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital output (PWM) pin number for which to write values.")]
        public int Pin { get; set; }

        public override IObservable<int> Process(IObservable<int> source)
        {
            return Observable.Using(
                () =>
                {
                    var analogOutput = ObservableArduino.AnalogOutput(PortName, Pin);
                    var iterator = analogOutput.GetEnumerator();
                    iterator.MoveNext();
                    return iterator;
                },
                iterator => source.Do(input => iterator.Current(input)));
        }
    }
}
