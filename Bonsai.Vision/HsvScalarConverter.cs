using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using OpenCV.Net;

namespace Bonsai.Vision
{
    class HsvScalarConverter : NumericRecordConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var baseProperties = base.GetProperties(context, value, attributes);

            var precisionAttribute = new PrecisionAttribute(0, 1);
            var editorAttribute = new EditorAttribute(DesignTypes.SliderEditor, DesignTypes.UITypeEditor);
            var hueAttributes = new Attribute[] { new RangeAttribute(0, 179), precisionAttribute, editorAttribute };
            var satValAttributes = new Attribute[] { new RangeAttribute(0, 255), precisionAttribute, editorAttribute };

            var properties = new PropertyDescriptor[3];
            properties[0] = new PropertyDescriptorWrapper("H", baseProperties["Val0"], hueAttributes);
            properties[1] = new PropertyDescriptorWrapper("S", baseProperties["Val1"], satValAttributes);
            properties[2] = new PropertyDescriptorWrapper("V", baseProperties["Val2"], satValAttributes);
            return new PropertyDescriptorCollection(properties);
        }
    }
}
