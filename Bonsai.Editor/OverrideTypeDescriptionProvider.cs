using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    class OverrideTypeDescriptionProvider : TypeDescriptionProvider
    {
        public OverrideTypeDescriptionProvider(TypeDescriptionProvider parent)
            : base(parent)
        {
        }

        public ICustomTypeDescriptor TypeDescriptor { get; set; }

        public ICustomTypeDescriptor ExtendedTypeDescriptor { get; set; }

        public override ICustomTypeDescriptor GetTypeDescriptor(Type objectType, object instance)
        {
            return TypeDescriptor ?? base.GetTypeDescriptor(objectType, instance);
        }

        public override ICustomTypeDescriptor GetExtendedTypeDescriptor(object instance)
        {
            return ExtendedTypeDescriptor ?? base.GetExtendedTypeDescriptor(instance);
        }
    }
}
