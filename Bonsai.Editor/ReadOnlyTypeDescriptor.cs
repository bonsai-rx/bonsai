using System.ComponentModel;

namespace Bonsai.Editor
{
    class ReadOnlyTypeDescriptor : CustomTypeDescriptor
    {
        public ReadOnlyTypeDescriptor(object instance, ICustomTypeDescriptor parent)
            : base(parent)
        {
            PropertyOwner = instance;
        }

        object PropertyOwner { get; }

        public override AttributeCollection GetAttributes()
        {
            var baseAttributes = base.GetAttributes();
            return AttributeCollection.FromExisting(baseAttributes, ReadOnlyAttribute.Yes);
        }

        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            return PropertyOwner;
        }
    }
}
