﻿using System;
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
                if (sourceBuilder != null) return sourceBuilder.Generator.GetType().Name;

                var conditionBuilder = expressionBuilder as ConditionBuilder;
                if (conditionBuilder != null) return conditionBuilder.Condition.GetType().Name;

                var whereBuilder = expressionBuilder as WhereBuilder;
                if (whereBuilder != null) return whereBuilder.Predicate.GetType().Name;

                var selectBuilder = expressionBuilder as SelectBuilder;
                if (selectBuilder != null) return selectBuilder.Selector.GetType().Name;

                var combinatorBuilder = expressionBuilder as CombinatorBuilder;
                if (combinatorBuilder != null) return combinatorBuilder.Combinator.GetType().Name;

                var workflowExpressionBuilder = expressionBuilder as WorkflowExpressionBuilder;
                if (workflowExpressionBuilder != null && !string.IsNullOrWhiteSpace(workflowExpressionBuilder.Name)) return workflowExpressionBuilder.Name;

                var type = expressionBuilder.GetType();
                return RemoveSuffix(type.Name, "Builder");
            }

            if (destinationType == typeof(Brush))
            {
                var expressionBuilder = value;
                var combinatorBuilder = expressionBuilder as CombinatorBuilder;
                if (combinatorBuilder != null) expressionBuilder = combinatorBuilder.Combinator;

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
