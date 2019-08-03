using Microsoft.CSharp;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class TypeMappingConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            var mapping = value as TypeMapping;
            if (mapping != null && mapping.TargetType != null && destinationType == typeof(string))
            {
                using (var provider = new CSharpCodeProvider())
                {
                    var typeRef = new CodeTypeReference(mapping.TargetType);
                    return provider.GetTypeOutput(typeRef);
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
