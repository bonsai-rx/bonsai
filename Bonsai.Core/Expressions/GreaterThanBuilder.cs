﻿using System.ComponentModel;
using System.Linq.Expressions;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies a "greater than" numeric comparison
    /// on paired elements of an observable sequence.
    /// </summary>
    [XmlType("GreaterThan", Namespace = Constants.XmlNamespace)]
    [Description("Applies a \"greater than\" numeric comparison on paired elements of an observable sequence.")]
    public class GreaterThanBuilder : BinaryOperatorBuilder
    {
        /// <summary>
        /// Returns the expression that applies a "greater than" numeric comparison
        /// to the left and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a "greater than" numeric comparison
        /// to the left and right parameters.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            if (left.Type.IsEnum && left.Type == right.Type)
            {
                left = Expression.Convert(left, left.Type.GetEnumUnderlyingType());
                right = Expression.Convert(right, right.Type.GetEnumUnderlyingType());
            }
            return Expression.GreaterThan(left, right);
        }
    }
}
