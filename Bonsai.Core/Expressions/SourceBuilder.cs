using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reflection;
using System.Reactive.Linq;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder which uses a specified source instance
    /// to generate an observable sequence.
    /// </summary>
    [Obsolete]
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Source", Namespace = Constants.XmlNamespace)]
    public class SourceBuilder : CombinatorExpressionBuilder, INamedElement
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SourceBuilder"/> class.
        /// </summary>
        public SourceBuilder()
            : base(minArguments: 0, maxArguments: 0)
        {
        }

        /// <summary>
        /// Gets the display name of the source.
        /// </summary>
        public string Name
        {
            get { return GetElementDisplayName(Generator); }
        }

        /// <summary>
        /// Gets or sets the source instance used to generate
        /// observable sequences.
        /// </summary>
        public object Generator { get; set; }

        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be combined with any
        /// existing property mappings to produce the final output of the expression builder.
        /// </summary>
        /// <param name="arguments">
        /// A collection of <see cref="Expression"/> nodes that represents the input arguments.
        /// </param>
        /// <returns>
        /// An <see cref="Expression"/> tree node that represents the source output.
        /// </returns>
        protected override Expression BuildCombinator(IEnumerable<Expression> arguments)
        {
            const BindingFlags bindingAttributes = BindingFlags.Instance | BindingFlags.Public;
            var sourceType = Generator.GetType();
            var sourceExpression = Expression.Constant(Generator);
            var sourceAttributes = sourceType.GetCustomAttributes(typeof(SourceAttribute), true);
            var methodName = ((SourceAttribute)sourceAttributes.Single()).MethodName;
            var generateMethod = sourceType.GetMethods(bindingAttributes)
                                           .Single(m => m.Name == methodName && m.GetParameters().Length == 0);
            return Expression.Call(sourceExpression, generateMethod);
        }
    }
}
