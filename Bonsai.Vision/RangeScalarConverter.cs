using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    class RangeScalarConverter : NumericRecordConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var baseProperties = base.GetProperties(context, value, attributes);
            var valueAttributes = new Attribute[] { new RangeAttribute(0, 255), new EditorAttribute(DesignTypes.SliderEditor, DesignTypes.UITypeEditor) };

            var properties = new PropertyDescriptor[4];
            properties[0] = new PropertyDescriptorWrapper("Val0", baseProperties["Val0"], valueAttributes);
            properties[1] = new PropertyDescriptorWrapper("Val1", baseProperties["Val1"], valueAttributes);
            properties[2] = new PropertyDescriptorWrapper("Val2", baseProperties["Val2"], valueAttributes);
            properties[3] = new PropertyDescriptorWrapper("Val3", baseProperties["Val3"], valueAttributes);
            return new PropertyDescriptorCollection(properties);
        }
    }
}
