using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace Bonsai
{
    /// <summary>
    /// Provides a type converter to convert two-dimensional <see cref="Array"/> objects
    /// to and from various other representations.
    /// </summary>
    public class MultidimensionalArrayConverter : TypeConverter
    {
        /// <summary>
        /// Returns whether this converter can convert an object of the given type to
        /// the type of this converter, using the specified context.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="sourceType">
        /// A <see cref="Type"/> that represents the type you want to convert from.
        /// </param>
        /// <returns>
        /// <b>true</b> if this converter can perform the conversion; otherwise, <b>false</b>.
        /// </returns>
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == typeof(string)) return true;
            return base.CanConvertFrom(context, sourceType);
        }

        /// <summary>
        /// Converts the given object to the type of this converter, using the specified
        /// context and culture information.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// The <see cref="CultureInfo"/> to use as the current culture.
        /// </param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            var valueString = value as string;
            if (valueString != null)
            {
                return ArrayConvert.ToArray(
                    valueString, 2,
                    context.PropertyDescriptor.PropertyType.GetElementType(),
                    culture);
            }

            return base.ConvertFrom(context, culture, value);
        }

        /// <summary>
        /// Converts the given value object to the specified type, using the specified
        /// context and culture information.
        /// </summary>
        /// <param name="context">
        /// An <see cref="ITypeDescriptorContext"/> that provides a format context.
        /// </param>
        /// <param name="culture">
        /// A <see cref="CultureInfo"/>. If <b>null</b> is passed, the current culture
        /// is assumed.
        /// </param>
        /// <param name="value">The <see cref="Object"/> to convert.</param>
        /// <param name="destinationType">The <see cref="Type"/> to convert the value parameter to.</param>
        /// <returns>An <see cref="Object"/> that represents the converted value.</returns>
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null && destinationType == typeof(string))
            {
                return ArrayConvert.ToString((Array)value, culture);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
