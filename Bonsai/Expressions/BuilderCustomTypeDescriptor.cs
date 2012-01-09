using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    class BuilderCustomTypeDescriptor : CustomTypeDescriptor
    {
        object element;

        public BuilderCustomTypeDescriptor(ICustomTypeDescriptor parent, object instance)
            : base(parent)
        {
            var loadableElement = instance.GetType().GetProperties().FirstOrDefault(property => typeof(LoadableElement).IsAssignableFrom(property.PropertyType));
            if (loadableElement != null)
            {
                element = loadableElement.GetValue(instance, null);
            }
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            var properties = base.GetProperties();
            if (element != null)
            {
                var attrs = new[] { new CategoryAttribute(element.GetType().Name) };
                var elementProperties = TypeDescriptor.GetProperties(element).Cast<PropertyDescriptor>().Select(property => new ProxyPropertyDescriptor(property, element, attrs)); ;
                return new PropertyDescriptorCollection(properties.Cast<PropertyDescriptor>().Concat(elementProperties).ToArray());
            }

            return properties;
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var properties = base.GetProperties(attributes);
            if (element != null)
            {
                var attrs = new[] { new CategoryAttribute(element.GetType().Name) };
                var elementProperties = TypeDescriptor.GetProperties(element, attributes).Cast<PropertyDescriptor>().Select(property => new ProxyPropertyDescriptor(property, element, attrs));
                return new PropertyDescriptorCollection(properties.Cast<PropertyDescriptor>().Concat(elementProperties).ToArray());
            }

            return properties;
        }

        class ProxyPropertyDescriptor : PropertyDescriptor
        {
            object propertyOwner;
            PropertyDescriptor parentDescriptor;

            public ProxyPropertyDescriptor(PropertyDescriptor parent, object instance, Attribute[] attrs)
                : base(parent, attrs)
            {
                propertyOwner = instance;
                parentDescriptor = parent;
            }

            public override bool CanResetValue(object component)
            {
                return parentDescriptor.CanResetValue(propertyOwner);
            }

            public override Type ComponentType
            {
                get { return parentDescriptor.ComponentType; }
            }

            public override object GetValue(object component)
            {
                return parentDescriptor.GetValue(propertyOwner);
            }

            public override bool IsReadOnly
            {
                get { return parentDescriptor.IsReadOnly; }
            }

            public override Type PropertyType
            {
                get { return parentDescriptor.PropertyType; }
            }

            public override void ResetValue(object component)
            {
                parentDescriptor.ResetValue(propertyOwner);
            }

            public override void SetValue(object component, object value)
            {
                parentDescriptor.SetValue(propertyOwner, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return parentDescriptor.ShouldSerializeValue(propertyOwner);
            }
        }
    }
}
