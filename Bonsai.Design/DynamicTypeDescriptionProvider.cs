using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Bonsai.Design
{
    public class DynamicTypeDescriptionProvider : TypeDescriptionProvider
    {
        public Collection<PropertyDescriptor> properties = new Collection<PropertyDescriptor>();

        public Collection<PropertyDescriptor> Properties
        {
            get { return properties; }
        }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            var baseDescriptor = TypeDescriptor.GetProvider(objectType).GetTypeDescriptor(instance);
            return baseDescriptor;
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            var dynamicDescriptor = new DynamicCustomTypeDescriptor(this);
            return dynamicDescriptor;
        }

        class DynamicCustomTypeDescriptor : CustomTypeDescriptor
        {
            DynamicTypeDescriptionProvider provider;

            public DynamicCustomTypeDescriptor(DynamicTypeDescriptionProvider provider)
            {
                this.provider = provider;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return new PropertyDescriptorCollection(provider.Properties.ToArray());
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return new PropertyDescriptorCollection(provider.Properties.ToArray());
            }
        }
    }
}
