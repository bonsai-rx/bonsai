using System.ComponentModel;

namespace Bonsai.IO
{
    /// <summary>
    /// Provides a type converter to convert serial baud rates to and from other representations.
    /// </summary>
    public class BaudRateConverter : Int32Converter
    {
        /// <inheritdoc/>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of standard serial baud rates.
        /// </summary>
        /// <returns>
        /// A <see cref="TypeConverter.StandardValuesCollection"/> containing a set of standard
        /// serial baud rates.
        /// </returns>
        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                9600,
                14400,
                19200,
                38400,
                57600,
                115200,
                128000
            });
        }
    }
}
