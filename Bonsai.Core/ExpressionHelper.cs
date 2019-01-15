using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using System.Globalization;
using System.Reactive;
using Bonsai.Expressions;

namespace Bonsai
{
    /// <summary>
    /// Provides a set of static methods to manipulate expression trees.
    /// </summary>
    public static class ExpressionHelper
    {
        /// <summary>
        /// Represents the character separating class members in a member selector expression.
        /// </summary>
        public const string MemberSeparator = ".";

        /// <summary>
        /// Represents the character separating selected members in a member selector expression.
        /// </summary>
        public const string ArgumentSeparator = ",";

        const char IndexBegin = '[';
        const char IndexEnd = ']';
        const char IndexArgumentSeparator = ',';

        /// <summary>
        /// Returns an array of <see cref="Type"/> objects that represent the bounded
        /// type parameters resulting from matching the specified generic type with a
        /// concrete type.
        /// </summary>
        /// <param name="genericType">The generic type definition used to test for bindings.</param>
        /// <param name="type">The <see cref="Type"/> used to bind against <paramref name="genericType"/>.</param>
        /// <returns>
        /// The array of <see cref="Type"/> objects representing the bounded type parameters,
        /// or an empty array, in case no compatible bindings are found.
        /// </returns>
        public static Type[] GetGenericTypeBindings(Type genericType, Type type)
        {
            return (from binding in ExpressionBuilder.GetParameterBindings(genericType, type)
                    select binding.Item1)
                    .ToArray();
        }

        /// <summary>
        /// Tests whether the specified type implements the generic enumerable interface.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to test.</param>
        /// <returns>
        /// <b>true</b> if the type implements the generic enumerable interface;
        /// otherwise, <b>false</b>.
        /// </returns>
        public static bool IsEnumerableType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            var enumerableBindings = ExpressionBuilder.GetParameterBindings(typeof(IEnumerable<>), type);
            return enumerableBindings.Any();
        }

        /// <summary>
        /// Tests whether the specified type implements a serialization compatible collection.
        /// </summary>
        /// <param name="type">The <see cref="Type"/> to test.</param>
        /// <returns>
        /// <b>true</b> if the type implements a serialization compatible collection;
        /// otherwise, <b>false</b>.
        /// </returns>
        public static bool IsCollectionType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return typeof(System.Collections.IList).IsAssignableFrom(type);
        }

        internal static Expression CreateTuple(Expression[] arguments)
        {
            return CreateTuple(arguments, 0);
        }

        internal static Expression CreateTuple(Expression[] arguments, int offset)
        {
            const int MaxLength = 7;
            var length = arguments.Length - offset;
            if (length > MaxLength)
            {
                var rest = CreateTuple(arguments, offset + MaxLength);
                var selectedArguments = new Expression[MaxLength + 1];
                selectedArguments[MaxLength] = rest;
                Array.Copy(arguments, offset, selectedArguments, 0, MaxLength);
                var memberTypes = Array.ConvertAll(selectedArguments, member => member.Type);
                var constructor = typeof(Tuple<,,,,,,,>).MakeGenericType(memberTypes).GetConstructors()[0];
                return Expression.New(constructor, selectedArguments);
            }
            else
            {
                if (offset > 0)
                {
                    var selectedArguments = new Expression[length];
                    Array.Copy(arguments, offset, selectedArguments, 0, length);
                    arguments = selectedArguments;
                }
                var memberTypes = Array.ConvertAll(arguments, member => member.Type);
                return Expression.Call(typeof(Tuple), "Create", memberTypes, arguments);
            }
        }

        /// <summary>
        /// Extracts the set of member accessor paths from a composite selector string.
        /// </summary>
        /// <param name="selector">
        /// The comma-separated selector string used to extract multiple members.
        /// </param>
        /// <returns>
        /// An enumerator of the set of member accessor paths extracted from
        /// the composite selector string.
        /// </returns>
        public static IEnumerable<string> SelectMemberNames(string selector)
        {
            if (string.IsNullOrEmpty(selector))
            {
                yield break;
            }

            var indexCounter = 0;
            var argumentBuilder = new StringBuilder();
            var trailingSeparator = Enumerable.Repeat(IndexArgumentSeparator, 1);
            foreach (var character in Enumerable.Concat(selector, trailingSeparator))
            {
                if (character != IndexArgumentSeparator || indexCounter > 0)
                {
                    if (character == IndexBegin) indexCounter++;
                    if (character == IndexEnd) indexCounter--;
                    argumentBuilder.Append(character);
                }
                else
                {
                    var argument = argumentBuilder.ToString().Trim();
                    if (argument.Length == 0)
                    {
                        throw new InvalidOperationException("Empty argument specification is not allowed.");
                    }

                    yield return argument;
                    argumentBuilder.Clear();
                }
            }

            if (indexCounter > 0)
            {
                throw new InvalidOperationException("Error parsing member specification: invalid format.");
            }
        }

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
            if (instance == null)
            {
                throw new ArgumentNullException("instance");
            }

            if (!string.IsNullOrWhiteSpace(memberPath) && memberPath != MemberSeparator)
            {
                foreach (var memberName in memberPath.Split(new[] { MemberSeparator }, StringSplitOptions.None))
                {
                    if (string.IsNullOrEmpty(memberName))
                    {
                        throw new ArgumentException("Member path contains invalid or duplicate member separator character.", "memberPath");
                    }

                    var indexBegin = memberName.IndexOf(IndexBegin);
                    if (indexBegin >= 0)
                    {
                        var indexEnd = memberName.IndexOf(IndexEnd);
                        if (indexEnd < 0) throw new ArgumentException("Member path has badly formatted index accessor.", "memberPath");
                        var propertyName = memberName.Substring(0, indexBegin);
                        var indexParameters = memberName
                            .Substring(indexBegin + 1, indexEnd - indexBegin - 1)
                            .Split(new[] { IndexArgumentSeparator }, StringSplitOptions.RemoveEmptyEntries);

                        var properties = instance.Type.GetProperties().Where(p =>
                            p.Name == propertyName &&
                            p.GetIndexParameters().Length == indexParameters.Length)
                            .ToArray();
                        if (properties.Length == 0) throw new ArgumentException("Member path has reference to non-existent indexed property.", "memberPath");
                        if (properties.Length > 1) throw new ArgumentException("Ambiguous indexed property access.", "memberPath");

                        var propertyInfo = properties[0];
                        var parameterInfo = propertyInfo.GetIndexParameters();
                        var arguments = (from indexParameter in parameterInfo.Zip(indexParameters, (xs, ys) => Tuple.Create(xs, ys))
                                         let parameterType = indexParameter.Item1.ParameterType
                                         let parameter = Convert.ChangeType(indexParameter.Item2, parameterType)
                                         select Expression.Constant(parameter))
                                        .ToArray();
                        instance = Expression.Property(instance, propertyInfo, arguments);
                    }
                    else instance = ExpressionHelper.PropertyOrField(instance, memberName);
                }
            }

            return instance;
        }

        static Expression PropertyOrField(Expression instance, string propertyOrFieldName)
        {
            if (instance.Type.IsInterface)
            {
                foreach (var type in Enumerable.Repeat(instance.Type, 1)
                                               .Concat(instance.Type.GetInterfaces()))
                {
                    var property = type.GetProperty(propertyOrFieldName);
                    if (property != null)
                    {
                        return Expression.Property(instance, property);
                    }
                }

                throw new ArgumentException(
                    string.Format("'{0}' is not a member of type '{1}'", propertyOrFieldName, instance.Type),
                    "propertyOrFieldName");
            }
            else return Expression.PropertyOrField(instance, propertyOrFieldName);
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
                        case 'l': tokenType = typeof(long); break;
                        case 'f': tokenType = typeof(float); break;
                        case 'd': tokenType = typeof(double); break;
                        case 'b': tokenType = typeof(bool); break;
                        case 'c': tokenType = typeof(char); break;
                        case 't': tokenType = typeof(DateTimeOffset); break;
                        case 'p': tokenType = typeof(TimeSpan); break;
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
                if (tokenType == typeof(int) || tokenType == typeof(long) ||
                    tokenType == typeof(float) || tokenType == typeof(double) ||
                    tokenType == typeof(DateTimeOffset) || tokenType == typeof(TimeSpan))
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
