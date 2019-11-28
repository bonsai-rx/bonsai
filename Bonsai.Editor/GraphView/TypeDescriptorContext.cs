using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor.GraphView
{
    class TypeDescriptorContext : ITypeDescriptorContext
    {
        IServiceProvider provider;

        public TypeDescriptorContext(object instance, PropertyDescriptor propertyDescriptor)
            : this(instance, propertyDescriptor, null)
        {
        }

        public TypeDescriptorContext(object instance, PropertyDescriptor propertyDescriptor, IServiceProvider serviceProvider)
        {
            Instance = instance;
            PropertyDescriptor = propertyDescriptor;
            provider = serviceProvider;
        }

        public virtual IContainer Container
        {
            get { return null; }
        }

        public object Instance { get; private set; }

        public virtual void OnComponentChanged()
        {
        }

        public virtual bool OnComponentChanging()
        {
            return true;
        }

        public PropertyDescriptor PropertyDescriptor { get; private set; }

        public virtual object GetService(Type serviceType)
        {
            if (provider != null)
            {
                return provider.GetService(serviceType);
            }

            return null;
        }
    }
}
