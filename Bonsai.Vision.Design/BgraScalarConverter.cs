﻿using System;
using System.ComponentModel;
using Bonsai.Design;
using OpenCV.Net;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type converter to convert scalar BGRA color values
    /// to and from various other representations.
    /// </summary>
    [Obsolete]
    public class BgraScalarConverter : TypeConverter
    {
        /// <inheritdoc/>
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <inheritdoc/>
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {
            var propertyDescriptor = context.PropertyDescriptor;
            var editorAttribute = propertyDescriptor.Attributes[typeof(EditorAttribute)] ?? new EditorAttribute(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor);
            var valueAttributes = new Attribute[] { propertyDescriptor.Attributes[typeof(PrecisionAttribute)], propertyDescriptor.Attributes[typeof(RangeAttribute)], editorAttribute };

            var properties = new PropertyDescriptor[4];
            properties[0] = new DynamicPropertyDescriptor("B", typeof(double), c => ((Scalar)c).Val0, (c, v) => { var s = (Scalar)c; s.Val0 = (double)v; propertyDescriptor.SetValue(context.Instance, s); }, valueAttributes);
            properties[1] = new DynamicPropertyDescriptor("G", typeof(double), c => ((Scalar)c).Val1, (c, v) => { var s = (Scalar)c; s.Val1 = (double)v; propertyDescriptor.SetValue(context.Instance, s); }, valueAttributes);
            properties[2] = new DynamicPropertyDescriptor("R", typeof(double), c => ((Scalar)c).Val2, (c, v) => { var s = (Scalar)c; s.Val2 = (double)v; propertyDescriptor.SetValue(context.Instance, s); }, valueAttributes);
            properties[3] = new DynamicPropertyDescriptor("A", typeof(double), c => ((Scalar)c).Val3, (c, v) => { var s = (Scalar)c; s.Val3 = (double)v; propertyDescriptor.SetValue(context.Instance, s); }, valueAttributes);

            var names = new[] { "B", "G", "R", "A" };
            return new PropertyDescriptorCollection(properties).Sort(names);
        }
    }
}
