using System;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    class PropertySourceTypeDescriptionProvider : TypeDescriptionProvider
    {
        static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(PropertySource));

        public PropertySourceTypeDescriptionProvider()
            : base(parentProvider)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (objectType != typeof(PropertySource) && instance != null)
            {
                var parentDescriptor = GetTypeDescriptor(typeof(PropertySource));
                return new PropertySourceTypeDescriptor(instance, parentDescriptor);
            }

            return base.GetTypeDescriptor(objectType, instance);
        }

        class PropertySourceTypeDescriptor : CustomTypeDescriptor
        {
            static readonly Attribute[] emptyAttributes = new Attribute[0];
            readonly PropertyDescriptorCollection parentProperties;
            readonly PropertyDescriptor valuePropertyDescriptor;
            readonly PropertySource propertySource;

            public PropertySourceTypeDescriptor(object instance, ICustomTypeDescriptor parent)
                : base(parent)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException(nameof(instance));
                }

                propertySource = (PropertySource)instance;
                parentProperties = TypeDescriptor.GetProperties(propertySource.ElementType);
                valuePropertyDescriptor = TypeDescriptor.GetProperties(propertySource.GetType())["Value"];
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(emptyAttributes);
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                Attribute[] propertyAttributes;
                var parentPropertyDescriptor = parentProperties[propertySource.MemberName];
                if (parentPropertyDescriptor != null)
                {
                    propertyAttributes = new Attribute[parentPropertyDescriptor.Attributes.Count];
                    parentPropertyDescriptor.Attributes.CopyTo(propertyAttributes, 0);
                }
                else propertyAttributes = emptyAttributes;
 
                var extendedProperty = new ExtendedPropertyDescriptor(valuePropertyDescriptor, propertyAttributes);
                return new PropertyDescriptorCollection(new[] { extendedProperty });
            }
        }
    }
}
