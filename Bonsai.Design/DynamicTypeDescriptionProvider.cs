using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.ObjectModel;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides dynamic supplemental metadata to a <see cref="TypeDescriptor"/>.
    /// </summary>
    public class DynamicTypeDescriptionProvider : TypeDescriptionProvider
    {
        readonly Collection<PropertyDescriptor> properties = new Collection<PropertyDescriptor>();

        /// <summary>
        /// Gets the collection of dynamic custom properties to be added to the
        /// <see cref="TypeDescriptor"/>.
        /// </summary>
        public Collection<PropertyDescriptor> Properties
        {
            get { return properties; }
        }

        /// <inheritdoc/>
        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return TypeDescriptor.GetProvider(objectType).GetTypeDescriptor(instance);
        }

        /// <inheritdoc/>
        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return new DynamicCustomTypeDescriptor(this);
        }

        class DynamicCustomTypeDescriptor : CustomTypeDescriptor
        {
            readonly DynamicTypeDescriptionProvider dynamicProvider;

            public DynamicCustomTypeDescriptor(DynamicTypeDescriptionProvider provider)
            {
                dynamicProvider = provider;
            }

            public override PropertyDescriptorCollection GetProperties()
            {
                return new PropertyDescriptorCollection(dynamicProvider.Properties.ToArray());
            }

            public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
            {
                return new PropertyDescriptorCollection(dynamicProvider.Properties.ToArray());
            }
        }
    }
}
