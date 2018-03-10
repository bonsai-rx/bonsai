using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class ExtendedPropertyDescriptor : PropertyDescriptor
    {
        readonly PropertyDescriptor descriptor;

        public ExtendedPropertyDescriptor(PropertyDescriptor descr, Attribute[] attrs)
            : base(descr, attrs)
        {
            descriptor = descr;
        }

        public override bool CanResetValue(object component)
        {
            return descriptor.CanResetValue(component);
        }

        public override Type ComponentType
        {
            get { return descriptor.ComponentType; }
        }

        public override object GetValue(object component)
        {
            return descriptor.GetValue(component);
        }

        public override bool IsReadOnly
        {
            get { return descriptor.IsReadOnly; }
        }

        public override Type PropertyType
        {
            get { return descriptor.PropertyType; }
        }

        public override void ResetValue(object component)
        {
            descriptor.ResetValue(component);
        }

        public override void SetValue(object component, object value)
        {
            descriptor.SetValue(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return descriptor.ShouldSerializeValue(component);
        }
    }
}
