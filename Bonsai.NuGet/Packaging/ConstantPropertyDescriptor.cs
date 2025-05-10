using System;
using System.ComponentModel;

namespace Bonsai.NuGet.Packaging
{
    public class ConstantPropertyDescriptor : PropertyDescriptor
    {
        readonly object constant;

        public ConstantPropertyDescriptor(string name, object value)
            : base(name, Array.Empty<Attribute>())
        {
            constant = value ?? throw new ArgumentNullException(nameof(value));
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
            return constant;
        }

        public override bool IsReadOnly
        {
            get { return true; }
        }

        public override Type PropertyType
        {
            get { return constant.GetType(); }
        }

        public override void ResetValue(object component)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object component, object value)
        {
            throw new NotSupportedException();
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }
}
