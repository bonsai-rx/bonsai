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
                    if (Array.Exists(filterProperties, name => property.Name == name)) continue;
                    activeProperties[count++] = property;
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
    }
}
