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
        /// Currently the only supported types are: int (%i); float (%f); bool (%b);
        /// char (%c); string (%s).
        /// </remarks>
        [TypeConverter(typeof(PatternConverter))]
        [Description("The parse pattern to match, including conversion specifications for output data types.")]
        public string Pattern { get; set; }

        static Expression BuildParser(Expression expression, string pattern)
        {
            var regexPattern = string.Empty;
            pattern = pattern.Replace("%%", "%");
            var tokens = pattern.Split(new[] { '%' }, StringSplitOptions.None);
            var tokenTypes = new Type[tokens.Length - 1];
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (i > 0)
                {
                    Type tokenType;
                    switch (token[0])
                    {
                        case 'i': tokenType = typeof(int); break;
                        case 'f': tokenType = typeof(float); break;
                        case 'b': tokenType = typeof(bool); break;
                        case 'c': tokenType = typeof(char); break;
                        case 's':
                        default:
                            tokenType = typeof(string);
                            break;
                    }

                    tokenTypes[i - 1] = tokenType;
                    token = token.Substring(1);
                    regexPattern += "(.*)";
                }

                regexPattern += token;
            }

            var regex = new Regex(regexPattern, RegexOptions.Singleline);
            var regexExpression = Expression.Constant(regex);
            var matchVariable = Expression.Variable(typeof(Match));
            var matchExpression = Expression.Call(regexExpression, "Match", null, expression);
            var matchAssignment = Expression.Assign(matchVariable, matchExpression);
            var matchResult = Expression.Property(matchVariable, "Success");

            var inputValidation = Expression.IfThen(
                Expression.Equal(expression, Expression.Constant(null, typeof(string))),
                Expression.Throw(Expression.Constant(
                    new InvalidOperationException("The input string cannot be null."))));

            int groupIndex = 1;
            var groupParsers = Array.ConvertAll(tokenTypes, tokenType =>
            {
                var groupExpression = Expression.Property(matchVariable, "Groups");
                var groupIndexer = Expression.Property(groupExpression, "Item", Expression.Constant(groupIndex));
                var groupValueExpression = Expression.Property(groupIndexer, "Value");
                var valueExpression = tokenType != typeof(string)
                    ? (Expression)Expression.Call(tokenType, "Parse", null, groupValueExpression)
                    : groupValueExpression;
                groupIndex++;
                return valueExpression;
            });

            var matchValidation = Expression.IfThen(
                Expression.Not(matchResult),
                Expression.Throw(Expression.Constant(
                    new InvalidOperationException("The input string failed to match the parse pattern."))));

            Expression resultExpression;
            switch (tokenTypes.Length)
            {
                case 0: resultExpression = Expression.Constant(Unit.Default); break;
                case 1: resultExpression = groupParsers[0]; break;
                default:
                    resultExpression = (Expression)Expression.Call(typeof(Tuple), "Create", tokenTypes, groupParsers);
                    break;
            }

            return Expression.Block(new[] { matchVariable },
                inputValidation,
                matchAssignment,
                matchValidation,
                resultExpression);
        }

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
            else return BuildParser(expression, pattern);
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
                    "%f",
                    "%b",
                    "%c",
                    "%s"
                });
            }
        }
    }
}
