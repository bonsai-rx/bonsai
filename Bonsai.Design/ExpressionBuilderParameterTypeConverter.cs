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
    public class ExpressionBuilderParameterTypeConverter : TypeConverter
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
                    var parameter = (ExpressionBuilderParameter)value;
                    return parameter.Value;
                }

                if (destinationType == typeof(Pen))
                {
                    var parameter = (ExpressionBuilderParameter)value;
                    var edge = context != null ? context.Instance as Edge<ExpressionBuilder, ExpressionBuilderParameter> : null;
                    if (edge != null)
                    {
                        var builder = edge.Target.Value;
                        var connectionIndex = parameter.GetEdgeConnectionIndex() - 1;
                        if (connectionIndex >= builder.ArgumentRange.UpperBound)
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
