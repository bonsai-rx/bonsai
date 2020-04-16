using System;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Arduino
{
    static class ObservableArduino
    {
        public static IObservable<int> AnalogInput(string portName, int pin)
        {
            return Observable.Create<int>(async observer =>
            {
                var connection = await ArduinoManager.ReserveConnectionAsync(portName);
                EventHandler<AnalogInputReceivedEventArgs> inputReceived;
                inputReceived = (sender, e) =>
                {
                    if (e.Pin == pin)
                    {
                        observer.OnNext(e.Value);
                    }
                };

                connection.Arduino.ReportAnalog(pin, true);
                connection.Arduino.AnalogInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    connection.Arduino.AnalogInputReceived -= inputReceived;
                    connection.Arduino.ReportAnalog(pin, false);
                    connection.Dispose();
                });
            });
        }

        public static IObservable<bool> DigitalInput(string portName, int pin)
        {
            return DigitalInput(portName, pin, PinMode.Input);
        }

        public static IObservable<bool> DigitalInput(string portName, int pin, PinMode pinMode)
        {
            return Observable.Create<bool>(async observer =>
            {
                var connection = await ArduinoManager.ReserveConnectionAsync(portName);
                connection.Arduino.PinMode(pin, pinMode);
                var port = Arduino.GetPortNumber(pin);
                EventHandler<DigitalInputReceivedEventArgs> inputReceived;
                inputReceived = (sender, e) =>
                {
                    if (e.Port == port)
                    {
                        observer.OnNext(connection.Arduino.DigitalRead(pin) != 0);
                    }
                };

                connection.Arduino.ReportDigital(port, true);
                connection.Arduino.DigitalInputReceived += inputReceived;
                return Disposable.Create(() =>
                {
                    connection.Arduino.DigitalInputReceived -= inputReceived;
                    connection.Arduino.ReportDigital(port, false);
                    connection.Dispose();
                });
            });
        }
    }
}
