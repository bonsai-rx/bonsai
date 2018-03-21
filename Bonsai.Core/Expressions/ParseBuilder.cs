using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies a pattern matching operation on
    /// elements of an observable sequence.
    /// </summary>
    [XmlType("Parse", Namespace = Constants.XmlNamespace)]
    [Description("Applies a pattern matching operation on elements of an observable sequence.")]
    public class ParseBuilder : SelectBuilder
    {
        /// <summary>
        /// Gets or sets the parse pattern to match, including conversion specifications
        /// for the different output data types.
        /// </summary>
        /// <remarks>
        /// The allowed conversion specifications are preceded by the character '%'.
        /// Currently the only supported types are: int (%i); float (%f); double (%d);
        /// bool (%b); char (%c); string (%s); date-time (%t) and time-span (%p).
        /// </remarks>
        [TypeConverter(typeof(PatternConverter))]
        [Description("The parse pattern to match, including conversion specifications for output data types.")]
        public string Pattern { get; set; }

        /// <summary>
        /// Returns the expression that applies a pattern matching operation on
        /// the specified input parameter to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that applies a pattern matching operation
        /// on the input parameter to the selector result.
        /// </returns>
        protected override Expression BuildSelector(Expression expression)
        {
            if (expression.Type != typeof(string))
            {
                throw new ArgumentException("Unsupported input data type. Parse input has to be of type 'string'.");
            }

            var pattern = Pattern;
            if (string.IsNullOrEmpty(pattern)) return Expression.Constant(Unit.Default);
            else return ExpressionHelper.Parse(expression, pattern);
        }

        class PatternConverter : StringConverter
        {
            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[]
                {
                    "%i",
                    "%l",
                    "%f",
                    "%d",
                    "%b",
                    "%c",
                    "%s",
                    "%t",
                    "%p"
                });
            }
        }
    }
}
