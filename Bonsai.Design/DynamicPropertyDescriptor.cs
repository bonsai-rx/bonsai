using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Design
{
    public class DynamicPropertyDescriptor<T> : PropertyDescriptor
    {
        Func<object, object> getter;
        Action<object, object> setter;

        public DynamicPropertyDescriptor(string name, Func<object, object> getter, Action<object, object> setter, params Attribute[] attributes)
            : base(name, attributes)
        {
            this.getter = getter;
            this.setter = setter;
        }

        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override Type ComponentType
        {
            get { return null; }
        }

        public override object GetValue(object component)
        {
            return getter(component);
        }

        public override bool IsReadOnly
        {
            get { return setter == null; }
        }

        public override Type PropertyType
        {
            get { return typeof(T); }
        }

        public override void ResetValue(object component)
        {
        }

        public override void SetValue(object component, object value)
        {
            if (setter == null) throw new InvalidOperationException("Tried to set a value on a read-only dynamic property.");

            setter(component, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return true;
        }
    }
}
