using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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
            : this()
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
            return string.Format("{0} ({1}{2}{3}{4})", BitsPerPixel, Red, Green, Blue, Alpha);
        }

        class ColorFormatConfigurationConverter : ExpandableObjectConverter
        {
            const string BitsPerPixelProperty = "BitsPerPixel";
            PropertyInfo[] propertyCache;
            int bppCache;

            PropertyInfo[] GetProperties(ITypeDescriptorContext context)
            {
                return propertyCache ?? (propertyCache = context.PropertyDescriptor.PropertyType.GetProperties(BindingFlags.Instance | BindingFlags.Public));
            }

            public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
            {
                var properties = GetProperties(context);
                var bppChanged = !propertyValues[BitsPerPixelProperty].Equals(bppCache);
                var instance = Activator.CreateInstance(context.PropertyDescriptor.PropertyType);
                foreach (var property in properties)
                {
                    if (property.Name == BitsPerPixelProperty && !bppChanged) continue;
                    var value = Convert.ChangeType(propertyValues[property.Name], property.PropertyType);
                    property.SetValue(instance, value);
                }

                bppCache = (int)propertyValues[BitsPerPixelProperty];
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
                bppCache = (int)properties[BitsPerPixelProperty].GetValue(value);
                return properties.Sort(new[] { "Red", "Green", "Blue", "Alpha", BitsPerPixelProperty });
            }
        }
    }
}
