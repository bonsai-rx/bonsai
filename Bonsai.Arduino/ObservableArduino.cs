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
        public static IEnumerable<Action<int>> AnalogOutput(string portName, int pin)
        {
            return AnalogOutput(portName, pin, PinMode.Pwm);
        }

        public static IEnumerable<Action<int>> AnalogOutput(string portName, int pin, PinMode pinMode)
        {
            using (var connection = ArduinoManager.ReserveConnection(portName))
            {
                connection.Arduino.PinMode(pin, pinMode);
                while (true)
                {
                    yield return value =>
                    {
                        lock (connection.Arduino)
                        {
                            connection.Arduino.AnalogWrite(pin, value);
                        }
                    };
                }
            }
        }

        public static IEnumerable<Action<bool>> DigitalOutput(string portName, int pin)
        {
            using (var connection = ArduinoManager.ReserveConnection(portName))
            {
                connection.Arduino.PinMode(pin, PinMode.Output);
                while (true)
                {
                    yield return value =>
                    {
                        lock (connection.Arduino)
                        {
                            connection.Arduino.DigitalWrite(pin, value ? Arduino.High : Arduino.Low);
                        };
                    };
                }
            }
        }

        public static IObservable<int> AnalogInput(string portName, int pin)
        {
            return Observable.Create<int>(observer =>
            {
                var connection = ArduinoManager.ReserveConnection(portName);
                EventHandler<AnalogInputReceivedEventArgs> inputReceived;
                inputReceived = (sender, e) =>
                {
                    if (e.Pin == pin)
                    {
                        observer.OnNext(e.Value);
                    }
                };

                connection.Arduino.AnalogInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    connection.Arduino.AnalogInputReceived -= inputReceived;
                    connection.Dispose();
                });
            });
        }

        public static IObservable<bool> DigitalInput(string portName, int pin)
        {
            return Observable.Create<bool>(observer =>
            {
                var connection = ArduinoManager.ReserveConnection(portName);
                connection.Arduino.PinMode(pin, PinMode.Input);
                var port = (pin >> 3) & 0x0F;
                EventHandler<DigitalInputReceivedEventArgs> inputReceived;
                inputReceived = (sender, e) =>
                {
                    if (e.Port == port)
                    {
                        observer.OnNext(connection.Arduino.DigitalRead(pin) != 0);
                    }
                };

                connection.Arduino.DigitalInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    connection.Arduino.DigitalInputReceived -= inputReceived;
                    connection.Dispose();
                });
            });
        }
    }
}
