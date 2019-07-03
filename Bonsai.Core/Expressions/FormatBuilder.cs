using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies a string formatting operation on
    /// elements of an observable sequence.
    /// </summary>
    [DefaultProperty("Selector")]
    [XmlType("Format", Namespace = Constants.XmlNamespace)]
    [Description("Applies a string formatting operation on elements of an observable sequence.")]
    public class FormatBuilder : SelectBuilder
    {
        static readonly MethodInfo formatMethod = typeof(string).GetMethod("Format", new[] {
            typeof(IFormatProvider),
            typeof(string),
            typeof(object[]) });

        /// <summary>
        /// Gets or sets the composite format string used to specify the output representation.
        /// </summary>
        [Editor(DesignTypes.MultilineStringEditor, DesignTypes.UITypeEditor)]
        [Description("The composite format string used to specify the output representation.")]
        public string Format { get; set; }

        /// <summary>
        /// Gets or sets a string used to specify the properties that will be included in the
        /// output representation.
        /// </summary>
        [Description("The inner properties that will be included in the output representation.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Selector { get; set; }

        /// <summary>
        /// Returns the expression that applies a string formatting operation on
        /// the specified input parameter to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a string formatting operation
        /// on the input parameter to the selector result.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            var format = Format;
            if (string.IsNullOrEmpty(format))
            {
                var toStringMethod = expression.Type.GetMethod("ToString", new[] { typeof(IFormatProvider) });
                if (toStringMethod != null)
                {
                    return Expression.Call(expression, toStringMethod, Expression.Constant(CultureInfo.InvariantCulture));
                }
                else return Expression.Call(expression, "ToString", null);
            }

            var formatExpression = Expression.Constant(format);
            var args = Expression.NewArrayInit(typeof(object), ExpressionHelper
                .SelectMembers(expression, Selector)
                .Select(x => Expression.Convert(x, typeof(object))));
            return Expression.Call(formatMethod, Expression.Constant(CultureInfo.InvariantCulture), formatExpression, args);
        }
    }
}
