using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reactive;

namespace Bonsai
{
    /// <summary>
    /// Provides a set of static methods to manipulate expression trees.
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Represents the character separating class members in a member selector
        /// <see cref="string"/>.
        /// </summary>
        public const string MemberSeparator = ".";

        const string IndexBegin = "[";
        const string IndexEnd = "]";
        const string IndexParameterSeparator = ",";

        /// <summary>
        /// Creates an <see cref="Expression"/> representing a chained access to a member
        /// variable.
        /// </summary>
        /// <param name="instance">The object to which the member chain belongs.</param>
        /// <param name="memberPath">
        /// The path to an inner member variable, separated by a dot. Indexed accessors
        /// are also allowed.
        /// </param>
        /// <returns>The created <see cref="Expression"/>.</returns>
        public static Expression MemberAccess(Expression instance, string memberPath)
        {
            if (!string.IsNullOrWhiteSpace(memberPath))
            {
                foreach (var memberName in memberPath.Split(new[] { MemberSeparator }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var indexBegin = memberName.IndexOf(IndexBegin);
                    if (indexBegin >= 0)
                    {
                        var indexEnd = memberName.IndexOf(IndexEnd);
                        if (indexEnd < 0) throw new ArgumentException("Member path has badly formatted index accessor.", "memberPath");
                        var propertyName = memberName.Substring(0, indexBegin);
                        var propertyInfo = instance.Type.GetProperty(propertyName);
                        if (propertyInfo == null) throw new ArgumentException("Member path has reference to non-existent indexed property.", "memberPath");
                        var parameterInfo = propertyInfo.GetIndexParameters();
                        var indexParameters = memberName
                            .Substring(indexBegin + 1, indexEnd - indexBegin - 1)
                            .Split(new[] { IndexParameterSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        var arguments = (from indexParameter in parameterInfo.Zip(indexParameters, (xs, ys) => Tuple.Create(xs, ys))
                                         let parameterType = indexParameter.Item1.ParameterType
                                         let parameter = Convert.ChangeType(indexParameter.Item2, parameterType)
                                         select Expression.Constant(parameter))
                                        .ToArray();
                        instance = Expression.Property(instance, propertyName, arguments);
                    }
                    else instance = Expression.PropertyOrField(instance, memberName);
                }
            }

            return instance;
        }

        /// <summary>
        /// Creates an <see cref="Expression"/> representing the result of parsing
        /// a string with the specified pattern.
        /// </summary>
        /// <param name="expression">
        /// An <see cref="Expression"/> that represents the string to parse.
        /// </param>
        /// <param name="pattern">
        /// The parse pattern to match, including conversion specifications
        /// for the different output data types.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> that represents the result of parsing
        /// the specified string.
        /// </returns>
        public static Expression Parse(Expression expression, string pattern)
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
            var invariantCulture = Expression.Constant(CultureInfo.InvariantCulture);

            var inputValidation = Expression.IfThen(
                Expression.Equal(expression, Expression.Constant(null, typeof(string))),
                Expression.Throw(Expression.Constant(
                    new InvalidOperationException("The input string cannot be null."))));

            int groupIndex = 1;
            var groupParsers = Array.ConvertAll(tokenTypes, tokenType =>
            {
                var groupExpression = Expression.Property(matchVariable, "Groups");
                var groupIndexer = Expression.Property(groupExpression, "Item", Expression.Constant(groupIndex++));
                var groupValueExpression = Expression.Property(groupIndexer, "Value");
                if (tokenType == typeof(string)) return (Expression)groupValueExpression;
                if (tokenType == typeof(int) || tokenType == typeof(float))
                {
                    return Expression.Call(tokenType, "Parse", null, groupValueExpression, invariantCulture);
                }
                else return Expression.Call(tokenType, "Parse", null, groupValueExpression);
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
    }
}
