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
    [Description("Processes each input window using the nested workflow and merges the result into a single sequence.")]
    public class SelectManyBuilder : WorkflowExpressionBuilder
    {
        static readonly MethodInfo returnMethod = (from method in typeof(Observable).GetMethods()
                                                   where method.Name == "Return" && method.GetParameters().Length == 1
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
            : base(workflow, minArguments: 1, maxArguments: 1)
        {
        }

        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be passed on
        /// to other builders in the workflow.
        /// </summary>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build()
        {
            var source = Arguments.Single();
            var sourceType = source.Type.GetGenericArguments()[0];

            // Assign input
            Expression inputParameter;
            var selectorParameter = Expression.Parameter(sourceType);
            if (!sourceType.IsGenericType || sourceType.GetGenericTypeDefinition() != typeof(IObservable<>))
            {
                inputParameter = Expression.Call(returnMethod.MakeGenericMethod(sourceType), selectorParameter);
            }
            else inputParameter = selectorParameter;

            return BuildWorflow(inputParameter, selectorBody =>
            {
                var selector = Expression.Lambda(selectorBody, selectorParameter);
                var selectorObservableType = selector.ReturnType.GetGenericArguments()[0];
                var selectManyGenericMethod = selectManyMethod.MakeGenericMethod(sourceType, selectorObservableType);
                return Expression.Call(selectManyGenericMethod, source, selector);
            });
        }
    }
}
