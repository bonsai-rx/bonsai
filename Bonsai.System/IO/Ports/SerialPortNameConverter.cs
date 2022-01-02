using System.ComponentModel;
using System.IO.Ports;

namespace Bonsai.IO
{
    /// <summary>
    /// Provides a type converter to convert serial port names to and from other representations.
    /// </summary>
    public class SerialPortNameConverter : StringConverter
    {
        /// <inheritdoc/>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of available serial port names for the current computer.
        /// </summary>
        /// <returns>
        /// A <see cref="TypeConverter.StandardValuesCollection"/> containing the set of
        /// available serial port names for the current computer.
        /// </returns>
        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(SerialPort.GetPortNames());
        }
    }
}
