using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Bonsai.Osc
{
    class TypeTagConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            return new StandardValuesCollection(new[]
            {
                Osc.TypeTag.Int32,
                Osc.TypeTag.Float,
                Osc.TypeTag.String,
                Osc.TypeTag.Blob,
                Osc.TypeTag.Int64,
                Osc.TypeTag.TimeTag,
                Osc.TypeTag.Double,
                Osc.TypeTag.Char
            });
        }
    }
}
