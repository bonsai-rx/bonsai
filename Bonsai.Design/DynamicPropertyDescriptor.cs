using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Bonsai.Design
{
    public class DynamicPropertyDescriptor : PropertyDescriptor
    {
        Type propertyType;
        Func<object, object> getter;
        Action<object, object> setter;

        public DynamicPropertyDescriptor(string name, Type propertyType, Func<object, object> getter, Action<object, object> setter, params Attribute[] attributes)
            : base(name, attributes)
        {
            this.propertyType = propertyType;
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
            get { return propertyType; }
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
