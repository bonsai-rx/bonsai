using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class ExternalizedPropertyTypeDescriptionProvider : TypeDescriptionProvider
    {
        static readonly TypeDescriptionProvider parentProvider = TypeDescriptor.GetProvider(typeof(ExternalizedProperty));

        public ExternalizedPropertyTypeDescriptionProvider()
            : base(parentProvider)
        {
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            if (objectType != typeof(ExternalizedProperty) && instance != null)
            {
                var parentDescriptor = base.GetTypeDescriptor(typeof(ExternalizedProperty));
                return new ExternalizedPropertyTypeDescriptor(instance, parentDescriptor);
            }

            return base.GetTypeDescriptor(objectType, instance);
        }

        class ExternalizedPropertyTypeDescriptor : CustomTypeDescriptor
        {
            PropertyDescriptorCollection parentProperties;
            ExternalizedProperty externalizedProperty;
            PropertyDescriptor externalizedDescriptor;
            static readonly Attribute[] emptyAttributes = new Attribute[0];

            public ExternalizedPropertyTypeDescriptor(object instance, ICustomTypeDescriptor parent)
                : base(parent)
            {
                if (instance == null)
                {
                    throw new ArgumentNullException("instance");
                }

                externalizedProperty = (ExternalizedProperty)instance;
                parentProperties = TypeDescriptor.GetProperties(externalizedProperty.ElementType);
                externalizedDescriptor = TypeDescriptor.GetProperties(externalizedProperty.GetType())["Value"];
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return GetProperties(emptyAttributes);
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                var properties = base.GetProperties(null);
                var propertyDescriptor = parentProperties[externalizedProperty.MemberName];

                Attribute[] externalizedAttributes;
                if (propertyDescriptor != null)
                {
                    externalizedAttributes = new Attribute[propertyDescriptor.Attributes.Count];
                    propertyDescriptor.Attributes.CopyTo(externalizedAttributes, 0);
                }
                else externalizedAttributes = new Attribute[0];
 
                var extendedProperty = new ExternalizedPropertyDescriptor(externalizedDescriptor, externalizedAttributes);
                var extendedProperties = new PropertyDescriptor[properties.Count + 1];
                properties.CopyTo(extendedProperties, 0);
                extendedProperties[extendedProperties.Length - 1] = extendedProperty;
                return new PropertyDescriptorCollection(extendedProperties);
            }
        }

        class ExternalizedPropertyDescriptor : PropertyDescriptor
        {
            PropertyDescriptor externalizedDescriptor;

            public ExternalizedPropertyDescriptor(PropertyDescriptor descr, Attribute[] attrs)
                : base(descr, attrs)
            {
                externalizedDescriptor = descr;
            }

            public override bool CanResetValue(object component)
            {
                return externalizedDescriptor.CanResetValue(component);
            }

            public override Type ComponentType
            {
                get { return externalizedDescriptor.ComponentType; }
            }

            public override object GetValue(object component)
            {
                return externalizedDescriptor.GetValue(component);
            }

            public override bool IsReadOnly
            {
                get { return externalizedDescriptor.IsReadOnly; }
            }

            public override Type PropertyType
            {
                get { return externalizedDescriptor.PropertyType; }
            }

            public override void ResetValue(object component)
            {
                externalizedDescriptor.ResetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                externalizedDescriptor.SetValue(component, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return externalizedDescriptor.ShouldSerializeValue(component);
            }
        }
    }
}
