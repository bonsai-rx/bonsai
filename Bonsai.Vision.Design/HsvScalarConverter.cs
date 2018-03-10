using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using Bonsai.Design;
using OpenCV.Net;
using System.Drawing.Design;

namespace Bonsai.Vision.Design
{
    [Obsolete]
    public class HsvScalarConverter : TypeConverter
    {
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var propertyDescriptor = context.PropertyDescriptor;
            var hueAttributes = new Attribute[] { new RangeAttribute(0, 179), new EditorAttribute(typeof(SliderEditor), typeof(UITypeEditor)) };
            var satValAttributes = new Attribute[] { new RangeAttribute(0, 255), new EditorAttribute(typeof(SliderEditor), typeof(UITypeEditor)) };

            var properties = new PropertyDescriptor[3];
            properties[0] = new DynamicPropertyDescriptor("H", typeof(double), c => ((Scalar)c).Val0, (c, v) => { var s = (Scalar)c; s.Val0 = (double)v; propertyDescriptor.SetValue(context.Instance, s); }, hueAttributes);
            properties[1] = new DynamicPropertyDescriptor("S", typeof(double), c => ((Scalar)c).Val1, (c, v) => { var s = (Scalar)c; s.Val1 = (double)v; propertyDescriptor.SetValue(context.Instance, s); }, satValAttributes);
            properties[2] = new DynamicPropertyDescriptor("V", typeof(double), c => ((Scalar)c).Val2, (c, v) => { var s = (Scalar)c; s.Val2 = (double)v; propertyDescriptor.SetValue(context.Instance, s); }, satValAttributes);

            return new PropertyDescriptorCollection(properties);
        }
    }
}
