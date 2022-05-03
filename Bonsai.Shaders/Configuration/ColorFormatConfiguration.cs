using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object specifying the color format of a draw buffer.
    /// </summary>
    [TypeConverter(typeof(ColorFormatConfigurationConverter))]
    public struct ColorFormatConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorFormatConfiguration"/>
        /// structure using the specified total number of bits per pixel.
        /// </summary>
        /// <param name="bpp">
        /// The total number of bits per pixel used by the color format.
        /// </param>
        public ColorFormatConfiguration(int bpp)
            : this()
        {
            BitsPerPixel = bpp;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ColorFormatConfiguration"/>
        /// structure using the specified number of bits per pixel for each channel.
        /// </summary>
        /// <param name="red">The number of bits per pixel for the red channel.</param>
        /// <param name="green">The number of bits per pixel for the green channel.</param>
        /// <param name="blue">The number of bits per pixel for the blue channel.</param>
        /// <param name="alpha">The number of bits per pixel for the alpha channel.</param>
        public ColorFormatConfiguration(int red, int green, int blue, int alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        /// <summary>
        /// Gets or sets a value specifying the number of bits per pixel for the
        /// red channel.
        /// </summary>
        [Description("Specifies the number of bits per pixel for the red channel.")]
        public int Red { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the number of bits per pixel for the
        /// green channel.
        /// </summary>
        [Description("Specifies the number of bits per pixel for the green channel.")]
        public int Green { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the number of bits per pixel for the
        /// blue channel.
        /// </summary>
        [Description("Specifies the number of bits per pixel for the blue channel.")]
        public int Blue { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the number of bits per pixel for the
        /// alpha channel.
        /// </summary>
        [Description("Specifies the number of bits per pixel for the alpha channel.")]
        public int Alpha { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the total number of bits per pixel.
        /// </summary>
        [XmlIgnore]
        [Description("Specifies the total number of bits per pixel.")]
        public int BitsPerPixel
        {
            get { return Red + Green + Blue + Alpha; }
            set { Red = Green = Blue = Alpha = value / 4; }
        }

        /// <summary>
        /// Returns a <see cref="string"/> that represents the current color format
        /// configuration.
        /// </summary>
        /// <returns>
        /// A <see cref="string"/> that represents the current color format
        /// configuration.
        /// </returns>
        public override string ToString()
        {
            return $"{BitsPerPixel} ({Red}{Green}{Blue}{Alpha})";
        }

        class ColorFormatConfigurationConverter : ExpandableObjectConverter
        {
            PropertyInfo[] propertyCache;
            int bppCache;

            PropertyInfo[] GetProperties(ITypeDescriptorContext context)
            {
                return propertyCache ??= context.PropertyDescriptor.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                var properties = GetProperties(context);
                var bppChanged = !propertyValues[nameof(BitsPerPixel)].Equals(bppCache);
                var instance = Activator.CreateInstance(context.PropertyDescriptor.PropertyType);
                foreach (var property in properties)
                {
                    if (property.Name == nameof(BitsPerPixel) && !bppChanged) continue;
                    var value = Convert.ChangeType(propertyValues[property.Name], property.PropertyType);
                    property.SetValue(instance, value);
                }

                bppCache = (int)propertyValues[nameof(BitsPerPixel)];
                return instance;
            }

            public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override bool GetPropertiesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
            {
                var properties = base.GetProperties(context, value, attributes);
                bppCache = (int)properties[nameof(BitsPerPixel)].GetValue(value);
                return properties.Sort(new[] { nameof(Red), nameof(Green), nameof(Blue), nameof(Alpha), nameof(BitsPerPixel) });
            }
        }
    }
}
