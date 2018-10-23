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
    [Obsolete]
    public class ExpressionBuilderTypeConverter : TypeConverter
    {
        static readonly Pen SolidPen = Pens.DarkGray;
        static readonly Pen DashPen = new Pen(Brushes.DarkGray)
        {
            DashPattern = new[] { 4f, 2f }
        };

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(ElementCategory)) return true;
            if (destinationType == typeof(Type)) return true;
            if (destinationType == typeof(Brush)) return true;
            if (destinationType == typeof(Pen)) return true;

            return base.CanConvertTo(context, destinationType);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType == typeof(string))
            {
                return ExpressionBuilder.GetElementDisplayName(value);
            }

            if (destinationType == typeof(Brush) || destinationType == typeof(ElementCategory))
            {
                var expressionBuilder = ExpressionBuilder.Unwrap((ExpressionBuilder)value);
                var elementAttributes = TypeDescriptor.GetAttributes(expressionBuilder);
                var elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                var obsolete = (ObsoleteAttribute)elementAttributes[typeof(ObsoleteAttribute)] != null;
                var disabled = expressionBuilder is DisableBuilder;

                var workflowElement = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
                if (workflowElement != expressionBuilder)
                {
                    var builderCategoryAttribute = elementCategoryAttribute;
                    elementAttributes = TypeDescriptor.GetAttributes(workflowElement);
                    elementCategoryAttribute = (WorkflowElementCategoryAttribute)elementAttributes[typeof(WorkflowElementCategoryAttribute)];
                    obsolete |= (ObsoleteAttribute)elementAttributes[typeof(ObsoleteAttribute)] != null;
                    if (elementCategoryAttribute == WorkflowElementCategoryAttribute.Default)
                    {
                        elementCategoryAttribute = builderCategoryAttribute;
                    }
                }

                if (destinationType == typeof(ElementCategory))
                {
                    return elementCategoryAttribute.Category;
                }

                switch (elementCategoryAttribute.Category)
                {
                    case ElementCategory.Source:
                        return disabled ? HatchBrushes.DiagonalViolet : obsolete ? HatchBrushes.CrossViolet : Brushes.Violet;
                    case ElementCategory.Condition:
                        return disabled ? HatchBrushes.DiagonalLightGreen : obsolete ? HatchBrushes.CrossLightGreen : Brushes.LightGreen;
                    case ElementCategory.Transform:
                        return disabled ? HatchBrushes.DiagonalWhite : obsolete ? HatchBrushes.CrossWhite : Brushes.White;
                    case ElementCategory.Sink:
                        return disabled ? HatchBrushes.DiagonalDarkGray : obsolete ? HatchBrushes.CrossDarkGray : Brushes.DarkGray;
                    case ElementCategory.Nested:
                    case ElementCategory.Workflow:
                        return disabled ? HatchBrushes.DiagonalGoldenrod : obsolete ? HatchBrushes.CrossGoldenrod : Brushes.Goldenrod;
                    case ElementCategory.Property:
                        return disabled ? HatchBrushes.DiagonalOrange : obsolete ? HatchBrushes.CrossOrange : Brushes.Orange;
                    case ElementCategory.Combinator:
                    default:
                        return disabled ? HatchBrushes.DiagonalLightBlue : obsolete ? HatchBrushes.CrossLightBlue : Brushes.LightBlue;
                }
            }

            if (destinationType == typeof(Type))
            {
                var expressionBuilder = (ExpressionBuilder)value;
                var workflowElement = ExpressionBuilder.GetWorkflowElement(expressionBuilder);
                return workflowElement.GetType();
            }

            if (destinationType == typeof(Pen))
            {
                var expressionBuilder = (ExpressionBuilder)value;
                return expressionBuilder.IsBuildDependency() ? DashPen : SolidPen;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
