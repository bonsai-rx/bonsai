using System;
using System.ComponentModel;

namespace Bonsai.Shaders
{
    class NullableEnumConverter : NullableConverter
    {
        public NullableEnumConverter(Type type)
            : base(type)
        {
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return false;
        }
    }
}
