using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using Bonsai.Expressions;
using Bonsai.Dag;

namespace Bonsai.Design
{
    [Obsolete]
    public class ExpressionBuilderArgumentTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Pen)) return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (value != null)
            {
                if (destinationType == typeof(string))
                {
                    var argument = (ExpressionBuilderArgument)value;
                    return argument.Name;
                }

                if (destinationType == typeof(Pen))
                {
                    var argument = (ExpressionBuilderArgument)value;
                    var edge = context != null ? context.Instance as Edge<ExpressionBuilder, ExpressionBuilderArgument> : null;
                    if (edge != null)
                    {
                        var builder = edge.Target.Value;
                        if (argument.Index >= builder.ArgumentRange.UpperBound)
                        {
                            return Pens.Red;
                        }
                    }

                    return Pens.Black;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
