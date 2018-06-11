using System;
using System.Collections.Generic;
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
    /// Represents an expression builder that combines higher-order observable sequences
    /// generated from the encapsulated workflow.
    /// </summary>
    public abstract class WorkflowCombinatorBuilder : SingleArgumentWorkflowExpressionBuilder
    {
        static readonly MethodInfo processMethod = typeof(WorkflowCombinatorBuilder).GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
                                                                                    .Single(m => m.Name == "Process");
        static readonly MethodInfo returnMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "Return" && method.GetParameters().Length == 1
                                                   select method)
                                                   .Single();

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowCombinatorBuilder"/> class.
        /// </summary>
        public WorkflowCombinatorBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WorkflowCombinatorBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public WorkflowCombinatorBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
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
            Expression inputParameter;
            var source = arguments.Single();
            var sourceType = source.Type.GetGenericArguments()[0];
            var selectorParameter = Expression.Parameter(sourceType);
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                inputParameter = Expression.Call(returnMethod.MakeGenericMethod(sourceType), selectorParameter);
            }
            else inputParameter = selectorParameter;

            return BuildWorkflow(arguments, inputParameter, selectorBody =>
            {
                var builder = Expression.Constant(this);
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                var processGenericMethod = processMethod.MakeGenericMethod(sourceType, selectorObservableType);
                return Expression.Call(builder, processGenericMethod, source, selector);
            });
        }

        internal abstract IObservable<TResult> Process<TSource, TResult>(IObservable<TSource> source, Func<TSource, IObservable<TResult>> selector);
    }
}
