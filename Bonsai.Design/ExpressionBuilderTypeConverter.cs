using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using Bonsai.Expressions;
using System.Drawing;

namespace Bonsai.Design
{
    public class ExpressionBuilderTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Brush)) return true;

            return base.CanConvertTo(context, destinationType);
        }

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

                var type = expressionBuilder.GetType();
                return type.Name.Remove(type.Name.LastIndexOf("Builder"));
            }

            if (destinationType == typeof(Brush))
            {
                var expressionBuilder = (ExpressionBuilder)value;

                var sourceBuilder = expressionBuilder as SourceBuilder;
                if (sourceBuilder != null) return Brushes.Violet;

                var whereBuilder = expressionBuilder as WhereBuilder;
                if (whereBuilder != null) return Brushes.White;

                var selectBuilder = expressionBuilder as SelectBuilder;
                if (selectBuilder != null) return Brushes.White;

                var doBuilder = expressionBuilder as DoBuilder;
                if (doBuilder != null) return Brushes.White;

                return Brushes.White;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
