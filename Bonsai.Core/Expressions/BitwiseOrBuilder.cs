﻿using System.ComponentModel;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies a bitwise OR operation
    /// on paired elements of an observable sequence.
    /// </summary>
    [XmlType("BitwiseOr", Namespace = Constants.XmlNamespace)]
    [Description("Applies a bitwise OR operation on paired elements of an observable sequence.")]
    public class BitwiseOrBuilder : BinaryOperatorBuilder
    {
        /// <summary>
        /// Returns the expression that applies a bitwise OR operation to the left
        /// and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a bitwise OR operation to the left
        /// and right parameters.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            if (left.Type.IsEnum && left.Type == right.Type)
            {
                left = Expression.Convert(left, left.Type.GetEnumUnderlyingType());
                right = Expression.Convert(right, right.Type.GetEnumUnderlyingType());
            }
            return Expression.Or(left, right);
        }
    }
}
