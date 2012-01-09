using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;

namespace Bonsai.Expressions
{
    public class ExpressionBuilderTypeConverter : TypeConverter
    {
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                var expressionBuilder = (ExpressionBuilder)value;

                var sourceBuilder = expressionBuilder as SourceBuilder;
                if (sourceBuilder != null) return sourceBuilder.Source.GetType().Name;

                var whereBuilder = expressionBuilder as WhereBuilder;
                if (whereBuilder != null) return whereBuilder.Filter.GetType().Name;

                var selectBuilder = expressionBuilder as SelectBuilder;
                if (selectBuilder != null) return selectBuilder.Projection.GetType().Name;

                var doBuilder = expressionBuilder as DoBuilder;
                if (doBuilder != null) return doBuilder.Sink.GetType().Name;

                return expressionBuilder.GetType().Name.Replace("Builder", string.Empty);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
