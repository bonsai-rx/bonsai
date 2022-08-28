using System;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Expressions
{
    class IncludeWorkflowXmlTypeDescriptor : CustomTypeDescriptor
    {
        readonly AttributeCollection attributes;
        readonly IncludeWorkflowBuilder includeBuilder;
        static readonly Attribute[] EmptyAttributes = new Attribute[0];

        public IncludeWorkflowXmlTypeDescriptor(IncludeWorkflowBuilder builder, params Attribute[] attrs)
        {
            attributes = new AttributeCollection(attrs ?? EmptyAttributes);
            includeBuilder = builder;
        }

        public override AttributeCollection GetAttributes()
        {
            return attributes;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(EmptyAttributes);
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var xmlProperties = includeBuilder.InternalXmlProperties;
            if (xmlProperties == null || xmlProperties.Length == 0) return PropertyDescriptorCollection.Empty;
            return new PropertyDescriptorCollection(xmlProperties
                .Select((element, index) => new XmlPropertyDescriptor(element.Name, index, DesignTimeVisibleAttribute.No))
                .ToArray());
        }
    }
}
