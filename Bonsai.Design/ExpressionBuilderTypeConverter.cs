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
            if (destinationType == typeof(Image)) return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ExpressionBuilder.GetElementDisplayName(value);
            }

            if (destinationType == typeof(Brush))
            {
                var expressionBuilder = ExpressionBuilder.Unwrap((ExpressionBuilder)value);
                var elementAttributes = TypeDescriptor.GetAttributes(expressionBuilder);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];

                var workflowElement = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
                if (workflowElement != expressionBuilder)
                {
                    var builderCategoryAttribute = elementCategoryAttribute;
                    elementAttributes = TypeDescriptor.GetAttributes(workflowElement);
                    elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                    if (elementCategoryAttribute == WorkflowElementCategoryAttribute.Default)
                    {
                        elementCategoryAttribute = builderCategoryAttribute;
                    }
                }

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
                    case ElementCategory.Property:
                        return Brushes.Orange;
                    case ElementCategory.Combinator:
                    default:
                        return Brushes.LightBlue;
                }
            }

            if (destinationType == typeof(Image))
            {
                var expressionBuilder = (ExpressionBuilder)value;
                var workflowElement = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
                var attributes = TypeDescriptor.GetAttributes(workflowElement);
                var bitmapAttribute = (ToolboxBitmapAttribute)attributes[typeof(ToolboxBitmapAttribute)];
                if (bitmapAttribute != ToolboxBitmapAttribute.Default)
                {
                    return bitmapAttribute.GetImage(value);
                }

                return null;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
