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
    [Description("Uses a sequence of numerical values from 0 to 180 to control a servomotor connected to an Arduino output pin.")]
    public class ServoOutput : Sink<int>
    {
        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        [Description("The digital output pin number to which the servo is connected.")]
        public int Pin { get; set; }

        public override IObservable<int> Process(IObservable<int> source)
        {
            return Observable.Using(
                () =>
                {
                    var analogOutput = ObservableArduino.AnalogOutput(PortName, Pin, PinMode.Servo);
                    var iterator = analogOutput.GetEnumerator();
                    iterator.MoveNext();
                    return iterator;
                },
                iterator => source.Do(input => iterator.Current(input)));
        }
    }
}
