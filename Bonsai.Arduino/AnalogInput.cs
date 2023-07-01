using System;
using System.ComponentModel;

namespace Bonsai.Arduino
{
    /// <summary>
    /// Represents an operator that generates a sequence of digitized analog readings
    /// from the specified Arduino input pin.
    /// </summary>
    [DefaultProperty(nameof(Pin))]
    [Description("Generates a sequence of digitized analog readings from the specified Arduino input pin.")]
    public class AnalogInput : Source<int>
    {
        /// <summary>
        /// Gets or sets the name of the serial port used to communicate with the Arduino.
        /// </summary>
        [TypeConverter(typeof(PortNameConverter))]
        [Description("The name of the serial port used to communicate with the Arduino.")]
        public string PortName { get; set; }

        /// <summary>
        /// Gets or sets the analog input pin number from which to take readings.
        /// </summary>
        [Description("The analog input pin number from which to take readings.")]
        public int Pin { get; set; }

        /// <summary>
        /// Generates an observable sequence of digitized analog values. 
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="int"/> values that report the digitized analog
        /// readings from the specified Arduino analog input pin.
        /// </returns>
        public override IObservable<int> Generate()
        {
            return ObservableArduino.AnalogInput(PortName, Pin);
        }
    }
}
