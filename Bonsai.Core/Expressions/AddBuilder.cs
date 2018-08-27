using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies the arithmetic addition operation
    /// on paired elements of an observable sequence.
    /// </summary>
    [XmlType("Add", Namespace = Constants.XmlNamespace)]
    [Description("Applies the arithmetic addition operation on paired elements of an observable sequence.")]
    public class AddBuilder : BinaryOperatorBuilder
    {
        static readonly MethodInfo stringConcat = typeof(string).GetMethod("Concat", new[] { typeof(string), typeof(string) });

        /// <summary>
        /// Returns the expression that applies the arithmetic addition operation
        /// to the left and right parameters.
        /// </summary>
        /// <param name="left">The left input parameter.</param>
        /// <param name="right">The right input parameter.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies the arithmetic addition operation
        /// to the left and right parameters.
        /// </returns>
        protected override Expression BuildSelector(Expression left, Expression right)
        {
            if (left.Type == typeof(string) && right.Type == typeof(string))
            {
                return Expression.Call(stringConcat, left, right);
            }
            else return Expression.Add(left, right);
        }
    }
}
