using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reflection;
using System.Reactive.Linq;
using System.ComponentModel;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that creates higher-order observable sequences
    /// using the encapsulated workflow.
    /// </summary>
    [XmlType("CreateObservable", Namespace = Constants.XmlNamespace)]
    [Description("Creates higher-order observable sequences using the encapsulated workflow.")]
    public class CreateObservableBuilder : WorkflowExpressionBuilder
    {
        static readonly Range<int> argumentRange = Range.Create(lowerBound: 0, upperBound: 1);
        static readonly MethodInfo returnMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "Return" && method.GetParameters().Length == 1
                                                   select method)
                                                   .Single();
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));
        static readonly MethodInfo deferMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Defer" &&
                                                                                m.GetParameters()[0].ParameterType
                                                                                 .GetGenericArguments()[0]
                                                                                 .GetGenericTypeDefinition() == typeof(IObservable<>));

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateObservableBuilder"/> class.
        /// </summary>
        public CreateObservableBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateObservableBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public CreateObservableBuilder(ExpressionBuilderGraph workflow)
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
            var source = arguments.FirstOrDefault();
            var inputParameter = default(Expression);
            var selectorParameter = default(ParameterExpression);
            if (source != null)
            {
                var sourceType = source.Type.GetGenericArguments()[0];
                selectorParameter = Expression.Parameter(sourceType);
                if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
                {
                    inputParameter = Expression.Call(returnMethod.MakeGenericMethod(sourceType), selectorParameter);
                }
                else inputParameter = selectorParameter;
            }

            return BuildWorkflow(arguments, inputParameter, selectorBody =>
            {
                if (selectorParameter != null)
                {
                    var selector = Expression.Lambda(selectorBody, selectorParameter);
                    return Expression.Call(selectMethod.MakeGenericMethod(selectorParameter.Type, selector.ReturnType), source, selector);
                }
                else
                {
                    var selector = Expression.Lambda(selectorBody);
                    var sourceType = selectorBody.Type.GetGenericArguments()[0];
                    selectorBody = Expression.Call(deferMethod.MakeGenericMethod(sourceType), selector);
                    return Expression.Call(returnMethod.MakeGenericMethod(selectorBody.Type), selectorBody);
                }
            });
        }
    }
}
