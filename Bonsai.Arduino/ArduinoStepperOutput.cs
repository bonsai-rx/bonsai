using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;

namespace Bonsai.Arduino
{
    public class ArduinoStepperOutput : Sink<int>
    {
        IEnumerable<Action<int, int>> stepperOutput;
        IEnumerator<Action<int, int>> iterator;

        [TypeConverter(typeof(SerialPortNameConverter))]
        public string SerialPort { get; set; }

        public int Stepper { get; set; }

        public int StepsPerRevolution { get; set; }

        public int DirectionPin { get; set; }

        public int StepPin { get; set; }

        public override void Process(int input)
        {
            iterator.Current(Math.Abs(input), input >= 0 ? 0 : 1);
        }

        public override IDisposable Load()
        {
            stepperOutput = ObservableArduino.StepperOutput(SerialPort, Stepper, StepsPerRevolution, DirectionPin, StepPin);
            iterator = stepperOutput.GetEnumerator();
            iterator.MoveNext();
            return base.Load();
        }

        protected override void Unload()
        {
            iterator.Dispose();
            stepperOutput = null;
            base.Unload();
        }
    }
}
