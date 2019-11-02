using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder which uses a specified combinator instance
    /// to process one or more input observable sequences.
    /// </summary>
    [XmlType("Combinator", Namespace = Constants.XmlNamespace)]
    public class CombinatorBuilder : CombinatorExpressionBuilder, INamedElement
    {
        object combinator;
        int maxArgumentCount;
        Delegate resetCombinator;

        /// <summary>
        /// Initializes a new instance of the <see cref="CombinatorBuilder"/> class.
        /// </summary>
        public CombinatorBuilder()
            : base(minArguments: 0, maxArguments: 0)
        {
        }

        /// <summary>
        /// Gets the display name of the combinator.
        /// </summary>
        public string Name
        {
            get { return GetElementDisplayName(combinator); }
        }

        /// <summary>
        /// Gets or sets the combinator instance used to process input
        /// observable sequences.
        /// </summary>
        public object Combinator
        {
            get { return combinator; }
            set
            {
                combinator = value;
                UpdateArgumentRange();
            }
        }

        void UpdateArgumentRange()
        {
            var combinatorBuilder = combinator as ExpressionBuilder;
            if (combinatorBuilder != null)
            {
                var range = combinatorBuilder.ArgumentRange;
                SetArgumentRange(range.LowerBound, range.UpperBound);
            }
            else
            {
                var combinatorType = combinator.GetType();
                resetCombinator = BuildResetCombinator(combinatorType);
                var processMethodParameters = GetProcessMethods(combinatorType).Select(m => m.GetParameters()).ToArray();
                var paramArray = processMethodParameters.Any(p =>
                    p.Length >= 1 &&
                    Attribute.IsDefined(p[p.Length - 1], typeof(ParamArrayAttribute)));

                if (paramArray) SetArgumentRange(1, maxArgumentCount = int.MaxValue);
                else
                {
                    var min = processMethodParameters.Min(p => p.Length);
                    var max = processMethodParameters.Max(p => p.Length);
                    SetArgumentRange(min, maxArgumentCount = max);
                }
            }
        }

        static Delegate BuildResetCombinator(Type combinatorType)
        {
            List<PropertyInfo> resetProperties = null;
            var combinatorProperties = combinatorType.GetProperties();
            for (int i = 0; i < combinatorProperties.Length; i++)
            {
                var property = combinatorProperties[i];
                if (!property.CanWrite || !Attribute.IsDefined(property, typeof(XmlIgnoreAttribute))) continue;

                var proxyProperty = Array.Find(combinatorProperties, p =>
                {
                    var xmlElement = p.GetCustomAttribute<XmlElementAttribute>();
                    return xmlElement != null && xmlElement.ElementName == property.Name;
                });
                if (proxyProperty == null)
                {
                    if (resetProperties == null) resetProperties = new List<PropertyInfo>();
                    resetProperties.Add(property);
                }
            }

            if (resetProperties == null) return null;
            var combinator = Expression.Parameter(combinatorType);
            return Expression.Lambda(Expression.Block(resetProperties.Select(property =>
            {
                var propertyExpression = Expression.Property(combinator, property);
                return Expression.Assign(propertyExpression, Expression.Default(property.PropertyType));
            })), combinator).Compile();
        }

        static IEnumerable<MethodInfo> GetProcessMethods(Type combinatorType)
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            var combinatorAttributes = combinatorType.GetCustomAttributes(typeof(CombinatorAttribute), true);
            var methodName = ((CombinatorAttribute)combinatorAttributes.Single()).MethodName;
            return combinatorType.GetMethods(bindingAttributes).Where(m => m.Name == methodName);
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node from a collection of input arguments.
        /// The result can be chained with other builders in a workflow.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var combinatorBuilder = combinator as ExpressionBuilder;
            if (combinatorBuilder != null)
            {
                return combinatorBuilder.Build(arguments);
            }

            return BuildCombinator(arguments);
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be combined with any
        /// existing property mappings to produce the final output of the expression builder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree node that represents the combinator output.
        /// </returns>
        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            var combinatorExpression = Expression.Constant(combinator);
            var processMethods = GetProcessMethods(combinatorExpression.Type);
            if (resetCombinator != null) resetCombinator.DynamicInvoke(combinator);
            return BuildCall(combinatorExpression, processMethods, arguments.Take(maxArgumentCount).ToArray());
        }
    }
}
