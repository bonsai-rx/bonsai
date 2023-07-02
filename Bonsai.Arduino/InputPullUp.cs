using System;
using System.ComponentModel;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that generates a sequence of digital state transitions
    /// from the specified Arduino input pin in pull-up mode.
    /// </summary>
    [DefaultProperty(nameof(Pin))]
    [Description("Generates a sequence of digital state transitions from the specified Arduino input pin in pull-up mode.")]
    public class InputPullUp : Source<bool>
    {
        /// <summary>
        /// Gets or sets the name of the serial port used to communicate with the Arduino.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the digital input pin number from which to take readings.
        /// </summary>
        [Description("The digital input pin number from which to take readings.")]
        public int Pin { get; set; }

        /// <summary>
        /// Configures the digital pin as INPUT_PULLUP and generates an observable
        /// sequence of all its state transitions.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="bool"/> values that report the binary state
        /// transitions of the specified Arduino input pin: <see langword="true"/>
        /// if the pin is now HIGH; <see langword="false"/> if the pin is now LOW.
        /// </returns>
        public override IObservable<bool> Generate()
        {
            return ObservableArduino.DigitalInput(PortName, Pin, PinMode.InputPullUp);
        }
    }
}
