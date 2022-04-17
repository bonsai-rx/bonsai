using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [TypeConverter(typeof(ColorFormatConfigurationConverter))]
    public struct ColorFormatConfiguration
    {
        public ColorFormatConfiguration(int bpp)
            : this()
        {
            BitsPerPixel = bpp;
        }

        public ColorFormatConfiguration(int red, int green, int blue, int alpha)
        {
            Red = red;
            Green = green;
            Blue = blue;
            Alpha = alpha;
        }

        [Description("The bits per pixel for the red channel.")]
        public int Red { get; set; }

        [Description("The bits per pixel for the green channel.")]
        public int Green { get; set; }

        [Description("The bits per pixel for the blue channel.")]
        public int Blue { get; set; }

        [Description("The bits per pixel for the alpha channel.")]
        public int Alpha { get; set; }

        [XmlIgnore]
        [Description("The total number of bits per pixel.")]
        public int BitsPerPixel
        {
            get { return Red + Green + Blue + Alpha; }
            set { Red = Green = Blue = Alpha = value / 4; }
        }

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
