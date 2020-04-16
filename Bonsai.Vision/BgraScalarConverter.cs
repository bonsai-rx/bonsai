using System;
using System.ComponentModel;

namespace Bonsai.Vision
{
    class BgraScalarConverter : NumericRecordConverter
    {
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var baseProperties = base.GetProperties(context, value, attributes);

            var propertyAttributes = context == null ? AttributeCollection.Empty : context.PropertyDescriptor.Attributes;
            var editorAttribute = propertyAttributes[typeof(EditorAttribute)] ?? new EditorAttribute(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor);
            var valueAttributes = new Attribute[] { propertyAttributes[typeof(PrecisionAttribute)], propertyAttributes[typeof(RangeAttribute)], editorAttribute };

            var properties = new PropertyDescriptor[4];
            properties[0] = new PropertyDescriptorWrapper("B", baseProperties["Val0"], valueAttributes);
            properties[1] = new PropertyDescriptorWrapper("G", baseProperties["Val1"], valueAttributes);
            properties[2] = new PropertyDescriptorWrapper("R", baseProperties["Val2"], valueAttributes);
            properties[3] = new PropertyDescriptorWrapper("A", baseProperties["Val3"], valueAttributes);

            var names = new[] { "B", "G", "R", "A" };
            return new PropertyDescriptorCollection(properties).Sort(names);
        }
    }
}
