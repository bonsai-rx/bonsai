﻿using System;
using System.ComponentModel;
using System.Globalization;
using System.Xml;
using System.Xml.Linq;

namespace Bonsai.Expressions
{
    class XmlPropertyDescriptor : PropertyDescriptor
    {
        readonly int propertyIndex;

        public XmlPropertyDescriptor(XName name, int index, params Attribute[] attrs)
            : base(name.LocalName, attrs)
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

        static XElement[] GetXmlProperties(object component)
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
            get { return typeof(XElement); }
        }

        public override void ResetValue(object component)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object component, object value)
        {
            if (!(value is XElement element))
            {
                throw new ArgumentException("Incompatible types found in workflow property assignment.", nameof(value));
            }

            var xmlProperties = GetXmlProperties(component);
            if (element.NodeType == XmlNodeType.Text)
            {
                element = new XProperty(xmlProperties[propertyIndex].Name, element.Value);
            }
            else if (element.Name != xmlProperties[propertyIndex].Name)
            {
                element = new XElement(
                    xmlProperties[propertyIndex].Name,
                    element.Attributes(),
                    element.Elements(),
                    element.Value);
            }

            xmlProperties[propertyIndex] = element;
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }

        class XProperty : XElement
        {
            static readonly XName DefaultName = XName.Get(typeof(XProperty).Name, Constants.XmlNamespace);

            internal XProperty(object content)
                : base(DefaultName, content)
            {
            }

            internal XProperty(XName name, object content)
                : base(name, content)
            {
            }

            public override XmlNodeType NodeType
            {
                get { return XmlNodeType.Text; }
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
                if (value is string data)
                {
                    return new XProperty(data);
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}
