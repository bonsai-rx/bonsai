using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Arduino
{
    static class ObservableArduino
    {
        static readonly Dictionary<string, ArduinoReference> openConnections = new Dictionary<string, ArduinoReference>();
        class ArduinoReference
        {
            public ArduinoReference(Arduino arduino)
            {
                Arduino = arduino;
            }

            public Arduino Arduino { get; private set; }

            public int RefCount { get; set; }
        }

        static Arduino ReserveConnection(string serialPort)
        {
            ArduinoReference arduinoReference;
            if (!openConnections.TryGetValue(serialPort, out arduinoReference))
            {
                var arduino = new Arduino(serialPort);
                arduinoReference = new ArduinoReference(arduino);
                openConnections.Add(serialPort, arduinoReference);
                arduino.Open();
            }

            arduinoReference.RefCount++;
            return arduinoReference.Arduino;
        }

        static void ReleaseConnection(string serialPort)
        {
            var arduinoReference = openConnections[serialPort];
            if (--arduinoReference.RefCount <= 0)
            {
                var arduino = arduinoReference.Arduino;
                arduino.Close();
                openConnections.Remove(serialPort);
            }
        }

        public static IEnumerable<Action<int>> AnalogOutput(string serialPort, int pin)
        {
            var arduino = ReserveConnection(serialPort);

            try
            {
                arduino.PinMode(pin, PinMode.Pwm);
                while (true)
                {
                    yield return value => arduino.AnalogWrite(pin, value);
                }
            }
            finally { ReleaseConnection(serialPort); }
        }

        public static IEnumerable<Action<bool>> DigitalOutput(string serialPort, int pin)
        {
            var arduino = ReserveConnection(serialPort);

            try
            {
                if (!arduino.IsOpen) arduino.Open();
                arduino.PinMode(pin, PinMode.Output);
                while (true)
                {
                    yield return value => arduino.DigitalWrite(pin, value ? Arduino.High : Arduino.Low);
                }
            }
            finally { ReleaseConnection(serialPort); }
        }

        public static IObservable<int> AnalogInput(string serialPort, int pin)
        {
            return Observable.Create<int>(observer =>
            {
                var arduino = ReserveConnection(serialPort);
                EventHandler<AnalogInputReceivedEventArgs> inputReceived;
                inputReceived = (sender, e) =>
                {
                    if (e.Pin == pin)
                    {
                        observer.OnNext(e.Value);
                    }
                };

                arduino.AnalogInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    arduino.AnalogInputReceived -= inputReceived;
                    ReleaseConnection(serialPort);
                });
            });
        }

        public static IObservable<bool> DigitalInput(string serialPort, int pin)
        {
            return Observable.Create<bool>(observer =>
            {
                var arduino = ReserveConnection(serialPort);
                arduino.PinMode(pin, PinMode.Input);
                var port = (pin >> 3) & 0x0F;
                EventHandler<DigitalInputReceivedEventArgs> inputReceived;
                inputReceived = (sender, e) =>
                {
                    if (e.Port == port)
                    {
                        observer.OnNext(arduino.DigitalRead(pin) != 0);
                    }
                };

                arduino.DigitalInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    arduino.DigitalInputReceived -= inputReceived;
                    ReleaseConnection(serialPort);
                });
            });
        }
    }
}
