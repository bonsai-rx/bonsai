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
        public static IEnumerable<Action<int>> AnalogOutput(string serialPort, int pin)
        {
            return AnalogOutput(serialPort, pin, PinMode.Pwm);
        }

        public static IEnumerable<Action<int>> AnalogOutput(string serialPort, int pin, PinMode pinMode)
        {
            var arduino = ArduinoManager.ReserveConnection(serialPort);

            try
            {
                arduino.PinMode(pin, pinMode);
                while (true)
                {
                    yield return value =>
                    {
                        lock (arduino)
                        {
                            arduino.AnalogWrite(pin, value);
                        }
                    };
                }
            }
            finally { ArduinoManager.ReleaseConnection(serialPort); }
        }

        public static IEnumerable<Action<bool>> DigitalOutput(string serialPort, int pin)
        {
            var arduino = ArduinoManager.ReserveConnection(serialPort);

            try
            {
                arduino.PinMode(pin, PinMode.Output);
                while (true)
                {
                    yield return value =>
                    {
                        lock (arduino)
                        {
                            arduino.DigitalWrite(pin, value ? Arduino.High : Arduino.Low);
                        };
                    };
                }
            }
            finally { ArduinoManager.ReleaseConnection(serialPort); }
        }

        public static IObservable<int> AnalogInput(string serialPort, int pin)
        {
            return Observable.Create<int>(observer =>
            {
                var arduino = ArduinoManager.ReserveConnection(serialPort);
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
                    ArduinoManager.ReleaseConnection(serialPort);
                });
            });
        }

        public static IObservable<bool> DigitalInput(string serialPort, int pin)
        {
            return Observable.Create<bool>(observer =>
            {
                var arduino = ArduinoManager.ReserveConnection(serialPort);
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
                    ArduinoManager.ReleaseConnection(serialPort);
                });
            });
        }
    }
}
