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
        static readonly Pen SolidPen = Pens.DarkGray;
        static readonly Pen DashPen = new Pen(Brushes.DarkGray)
        {
            DashPattern = new[] { 4f, 2f }
        };

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(Brush)) return true;
            if (destinationType == typeof(Image)) return true;
            if (destinationType == typeof(Pen)) return true;

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

                switch (elementCategoryAttribute.Category)
                {
                    case ElementCategory.Source:
                        return obsolete ? HatchBrushes.DiagonalViolet : disabled ? HatchBrushes.CrossViolet : Brushes.Violet;
                    case ElementCategory.Condition:
                        return obsolete ? HatchBrushes.DiagonalLightGreen : disabled ? HatchBrushes.CrossLightGreen : Brushes.LightGreen;
                    case ElementCategory.Transform:
                        return obsolete ? HatchBrushes.DiagonalWhite : disabled ? HatchBrushes.CrossWhite : Brushes.White;
                    case ElementCategory.Sink:
                        return obsolete ? HatchBrushes.DiagonalDarkGray : disabled ? HatchBrushes.CrossDarkGray : Brushes.DarkGray;
                    case ElementCategory.Nested:
                        return obsolete ? HatchBrushes.DiagonalGoldenrod : disabled ? HatchBrushes.CrossGoldenrod : Brushes.Goldenrod;
                    case ElementCategory.Property:
                        return obsolete ? HatchBrushes.DiagonalOrange : disabled ? HatchBrushes.CrossOrange : Brushes.Orange;
                    case ElementCategory.Combinator:
                    default:
                        return obsolete ? HatchBrushes.DiagonalLightBlue : disabled ? HatchBrushes.CrossLightBlue : Brushes.LightBlue;
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

            if (destinationType == typeof(Pen))
            {
                var expressionBuilder = (ExpressionBuilder)value;
                return expressionBuilder.IsBuildDependency() ? DashPen : SolidPen;
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}
