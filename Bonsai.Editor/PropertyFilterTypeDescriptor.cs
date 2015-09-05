using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    class PropertyFilterTypeDescriptor : CustomTypeDescriptor
    {
        readonly string[] filterProperties;

        internal PropertyFilterTypeDescriptor(ICustomTypeDescriptor parent, string[] filterProperties)
            : base(parent)
        {
            this.filterProperties = filterProperties;
        }

        PropertyDescriptorCollection GetFilteredProperties(PropertyDescriptorCollection properties)
        {
            if (filterProperties != null)
            {
                var count = 0;
                var activeProperties = new PropertyDescriptor[properties.Count];
                foreach (PropertyDescriptor property in properties)
                {
                    if (Array.Exists(filterProperties, name => property.Name == name))
                    {
                        activeProperties[count++] = new ReadOnlyPropertyDescriptor(property);
                    }
                    else activeProperties[count++] = property;
                }
                Array.Resize(ref activeProperties, count);
                return new PropertyDescriptorCollection(activeProperties);
            }

            return properties;
        }

        public override PropertyDescriptorCollection GetProperties()
        {
            return GetFilteredProperties(base.GetProperties());
        }

        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            return GetFilteredProperties(base.GetProperties(attributes));
        }

        class ReadOnlyPropertyDescriptor : PropertyDescriptor
        {
            readonly PropertyDescriptor parent;

            public ReadOnlyPropertyDescriptor(PropertyDescriptor descr)
                : base(descr)
            {
                parent = descr;
            }

            public override bool CanResetValue(object component)
            {
                return parent.CanResetValue(component);
            }

            public override Type ComponentType
            {
                get { return parent.ComponentType; }
            }

            public override object GetValue(object component)
            {
                return parent.GetValue(component);
            }

            public override bool IsReadOnly
            {
                get { return true; }
            }

            public override Type PropertyType
            {
                get { return parent.PropertyType; }
            }

            public override void ResetValue(object component)
            {
                parent.ResetValue(component);
            }

            public override void SetValue(object component, object value)
            {
                parent.SetValue(component, value);
            }

            public override bool ShouldSerializeValue(object component)
            {
                return parent.ShouldSerializeValue(component);
            }
        }
    }
}
