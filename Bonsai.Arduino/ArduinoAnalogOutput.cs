﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.IO;
using System.Drawing.Design;

namespace Bonsai.Arduino
{
    public class ArduinoAnalogOutput : Sink<int>
    {
        IEnumerable<Action<int>> analogOutput;
        IEnumerator<Action<int>> iterator;

        [Editor("Bonsai.Arduino.Design.ArduinoConfigurationEditor, Bonsai.Arduino.Design", typeof(UITypeEditor))]
        public string SerialPort { get; set; }

        public int Pin { get; set; }

        public override void Process(int input)
        {
            iterator.Current(input);
        }

        public override IDisposable Load()
        {
            analogOutput = ObservableArduino.AnalogOutput(SerialPort, Pin);
            iterator = analogOutput.GetEnumerator();
            iterator.MoveNext();
            return base.Load();
        }

        protected override void Unload()
        {
            iterator.Dispose();
            analogOutput = null;
            base.Unload();
        }
    }
}
