using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that creates a new observable sequence
    /// for each subscription using the encapsulated workflow.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Source)]
    [XmlType("Defer", Namespace = Constants.XmlNamespace)]
    [Description("Creates a new observable sequence for each subscription using the encapsulated workflow.")]
    public class DeferBuilder : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 0);
        static readonly MethodInfo deferMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Defer" &&
                                                                                m.GetParameters()[0].ParameterType
                                                                                 .GetGenericArguments()[0]
                                                                                 .GetGenericTypeDefinition() == typeof(IObservable<>));

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferBuilder"/> class.
        /// </summary>
        public DeferBuilder()
            : base(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeferBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public DeferBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        /// <summary>
        /// Gets the range of input arguments that this expression builder accepts.
        /// </summary>
        public override Range<int> ArgumentRange
        {
            get { return argumentRange; }
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
            return BuildWorkflow(arguments, null, selectorBody =>
            {
                var factory = Expression.Lambda(selectorBody);
                var resultType = selectorBody.Type.GetGenericArguments()[0];
                return Expression.Call(deferMethod.MakeGenericMethod(resultType), factory);
            });
        }
    }
}
