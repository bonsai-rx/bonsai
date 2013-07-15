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
        string RemoveSuffix(string source, string suffix)
        {
            var suffixStart = source.LastIndexOf(suffix);
            return suffixStart >= 0 ? source.Remove(suffixStart) : source;
        }

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
                if (whereBuilder != null) return whereBuilder.Condition.GetType().Name;

                var selectBuilder = expressionBuilder as SelectBuilder;
                if (selectBuilder != null) return selectBuilder.Transform.GetType().Name;

                var doBuilder = expressionBuilder as DoBuilder;
                if (doBuilder != null) return doBuilder.Sink.GetType().Name;

                var workflowExpressionBuilder = expressionBuilder as WorkflowExpressionBuilder;
                if (workflowExpressionBuilder != null && !string.IsNullOrWhiteSpace(workflowExpressionBuilder.Name)) return workflowExpressionBuilder.Name;

                var type = expressionBuilder.GetType();
                return RemoveSuffix(type.Name, "Builder");
            }

            if (destinationType == typeof(Brush))
            {
                var expressionBuilder = (ExpressionBuilder)value;
                var elementAttributes = TypeDescriptor.GetAttributes(expressionBuilder);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                switch (elementCategoryAttribute.Category)
                {
                    case ElementCategory.Source:
                        return Brushes.Violet;
                    case ElementCategory.Condition:
                        return Brushes.LightGreen;
                    case ElementCategory.Transform:
                        return Brushes.White;
                    case ElementCategory.Sink:
                        return Brushes.Gray;
                    case ElementCategory.Nested:
                        return Brushes.Goldenrod;
                    case ElementCategory.Combinator:
                    default:
                        return Brushes.LightBlue;
                }
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
