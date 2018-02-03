using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var parentDescriptor = base.GetTypeDescriptor(typeof(PropertySource));
                return new PropertySourceTypeDescriptor(instance, parentDescriptor);
            }

            return base.GetTypeDescriptor(objectType, instance);
        }

        class PropertySourceTypeDescriptor : CustomTypeDescriptor
        {
            static readonly Attribute[] emptyAttributes = new Attribute[0];
            PropertyDescriptorCollection parentProperties;
            PropertyDescriptor valuePropertyDescriptor;
            PropertySource propertySource;

            public PropertySourceTypeDescriptor(object instance, ICustomTypeDescriptor parent)
                : base(parent)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
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
