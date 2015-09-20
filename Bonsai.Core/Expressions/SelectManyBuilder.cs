using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Linq.Expressions;
using Bonsai.Dag;
using System.Reactive.Linq;
using System.Reflection;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that generates an expression tree by applying the
    /// SelectMany operator using an encapsulated workflow selector to an observable sequence
    /// of windows.
    /// </summary>
    [XmlType("SelectMany", Namespace = Constants.XmlNamespace)]
    [Description("Processes each input window using the encapsulated workflow and merges the result into a single sequence.")]
    public class SelectManyBuilder : WorkflowExpressionBuilder
    {
        static readonly MethodInfo returnMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "Return" && method.GetParameters().Length == 1
                                                   select method)
                                                   .Single();
        static readonly MethodInfo toObservableMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "ToObservable" && method.GetParameters().Length == 1
                                                   select method)
                                                   .Single();
        static readonly MethodInfo selectManyMethod = (from method in typeof(Observable).GetMethods()
                                                       where method.Name == "SelectMany"
                                                       let parameters = method.GetParameters()
                                                       where parameters.Length == 2
                                                       let selectorType = parameters[1].ParameterType
                                                       where selectorType.IsGenericType && selectorType.GetGenericTypeDefinition() == typeof(Func<,>) &&
                                                             selectorType.GetGenericArguments()[1].GetGenericTypeDefinition() == typeof(IObservable<>)
                                                       select method)
                                                      .Single();

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectManyBuilder"/> class.
        /// </summary>
        public SelectManyBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SelectManyBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public SelectManyBuilder(ExpressionBuilderGraph workflow)
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
            var source = arguments.FirstOrDefault();
            if (source == null)
            {
                throw new InvalidOperationException("There must be at least one workflow input to SelectMany.");
            }

            // Assign input
            Expression inputParameter;
            var sourceType = source.Type.GetGenericArguments()[0];
            var selectorParameter = Expression.Parameter(sourceType);
            var enumerableBindings = GetParameterBindings(typeof(IEnumerable<>), sourceType).FirstOrDefault();
            if (enumerableBindings != null && sourceType != typeof(string))
            {
                inputParameter = Expression.Call(toObservableMethod.MakeGenericMethod(enumerableBindings.Item1), selectorParameter);
            }
            else if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                inputParameter = Expression.Call(returnMethod.MakeGenericMethod(sourceType), selectorParameter);
            }
            else inputParameter = selectorParameter;

            return BuildWorkflow(arguments, inputParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                var selectManyGenericMethod = selectManyMethod.MakeGenericMethod(sourceType, selectorObservableType);
                return Expression.Call(selectManyGenericMethod, source, selector);
            });
        }
    }
}
