using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that applies a pattern matching operation on
    /// elements of an observable sequence.
    /// </summary>
    [DefaultProperty("Pattern")]
    [XmlType("Parse", Namespace = Constants.XmlNamespace)]
    [Description("Applies a pattern matching operation on elements of an observable sequence.")]
    public class ParseBuilder : SelectBuilder
    {
        static readonly string[] EmptySeparator = new string[0];

        /// <summary>
        /// Gets or sets the parse pattern to match, including data type format specifiers.
        /// </summary>
        /// <remarks>
        /// The allowed data type format specifiers are preceded by the character '%'.
        /// Currently the supported types are: byte (%B); short (%h); int (%i); long (%l); float (%f);
        /// double (%d); bool (%b); char (%c); string (%s); date-time (%t) and time-span (%T).
        /// Upper case characters can be used to indicate the unsigned type in the case of
        /// integer elements (e.g. %I for unsigned int).
        /// </remarks>
        [TypeConverter(typeof(PatternConverter))]
        [Editor("Bonsai.Design.ParsePatternEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The parse pattern to match, including data type format specifiers. If the pattern is empty, the input string is returned.")]
        public string Pattern { get; set; }

        /// <summary>
        /// Gets or sets the optional separator used to delimit elements in variable
        /// length patterns.
        /// </summary>
        [Description("The separator used to delimit elements in variable length patterns. This argument is optional.")]
        public string Separator { get; set; }

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
            var pattern = Pattern;
            var separatorString = Separator;
            var separator = string.IsNullOrEmpty(separatorString) ? EmptySeparator : new[] { Regex.Unescape(separatorString) };
            if (string.IsNullOrEmpty(pattern)) pattern = null;
            return ExpressionHelper.Parse(expression, pattern, separator);
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
                    "%B",
                    "%h",
                    "%H",
                    "%i",
                    "%I",
                    "%l",
                    "%L",
                    "%f",
                    "%d",
                    "%b",
                    "%c",
                    "%s",
                    "%t",
                    "%T"
                });
            }
        }
    }
}
