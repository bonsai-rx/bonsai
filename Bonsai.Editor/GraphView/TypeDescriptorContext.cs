using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    class TypeDescriptorContext : ITypeDescriptorContext
    {
        public TypeDescriptorContext(object instance, PropertyDescriptor propertyDescriptor)
        {
            Instance = instance;
            PropertyDescriptor = propertyDescriptor;
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
            return false;
        }

        public PropertyDescriptor PropertyDescriptor { get; private set; }

        public virtual object GetService(Type serviceType)
        {
            return null;
        }
    }
}
