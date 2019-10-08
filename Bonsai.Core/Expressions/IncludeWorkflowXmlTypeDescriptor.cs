using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Bonsai.Expressions
{
    class IncludeWorkflowXmlTypeDescriptor : CustomTypeDescriptor
    {
        AttributeCollection attributes;
        IncludeWorkflowBuilder includeBuilder;
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

        class XmlPropertyDescriptor : PropertyDescriptor
        {
            int propertyIndex;

            public XmlPropertyDescriptor(string name, int index, params Attribute[] attrs)
                : base(name, attrs)
            {
                propertyIndex = index;
            }

            public override TypeConverter Converter
            {
                get { return XmlPropertyConverter.Default; }
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override Type ComponentType
            {
                get { return typeof(IncludeWorkflowBuilder); }
            }

            static XmlNode[] GetXmlProperties(object component)
            {
                return ((IncludeWorkflowBuilder)component).InternalXmlProperties;
            }

            public override object GetValue(object component)
            {
                return GetXmlProperties(component)[propertyIndex];
            }

            public override bool IsReadOnly
            {
                get { return false; }
            }

            public override Type PropertyType
            {
                get { return typeof(XmlNode); }
            }

            public override void ResetValue(object component)
            {
                throw new NotSupportedException();
            }

            public override void SetValue(object component, object value)
            {
                var xmlNode = value as XmlNode;
                if (xmlNode == null)
                {
                    throw new ArgumentException("Incompatible types found in workflow property assignment.", "value");
                }

                var xmlProperties = GetXmlProperties(component);
                if (xmlNode.NodeType == XmlNodeType.CDATA)
                {
                    var property = xmlProperties[propertyIndex];
                    xmlNode = new XmlCDataProperty(property, xmlNode.Value);
                }

                xmlProperties[propertyIndex] = xmlNode;
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }
        }

        class XmlCDataProperty : XmlCDataSection
        {
            string name;
            static readonly XmlDocument EmptyDocument = new XmlDocument();

            internal XmlCDataProperty(string value)
                : this(string.Empty, value, EmptyDocument)
            {
            }

            internal XmlCDataProperty(XmlNode node, string value)
                : this(node.Name, value, EmptyDocument)
            {
            }

            internal XmlCDataProperty(string name, string value, XmlDocument doc)
                : base(value, doc)
            {
                this.name = name;
            }

            public override string Name
            {
                get { return name; }
            }

            public override XmlNodeType NodeType
            {
                get { return XmlNodeType.CDATA; }
            }
        }

        class XmlPropertyConverter : TypeConverter
        {
            internal static readonly XmlPropertyConverter Default = new XmlPropertyConverter();

            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                var data = value as string;
                if (data != null)
                {
                    return new XmlCDataProperty(data);
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
