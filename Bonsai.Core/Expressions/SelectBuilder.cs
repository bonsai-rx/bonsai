using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Provides a base class for expression builders that define a simple selector on the
    /// elements of an observable sequence. This is an abstract class.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class SelectBuilder : SingleArgumentExpressionBuilder
    {
        static readonly MethodInfo selectMethod = typeof(Observable).GetMethods()
                                                                    .Single(m => m.Name == "Select" &&
                                                                            m.GetParameters().Length == 2 &&
                                                                            m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        /// <summary>
        /// When overridden in a derived class, returns the expression
        /// that maps the specified input parameter to the selector result.
        /// </summary>
        /// <param name="expression">The input parameter to the selector.</param>
        /// <returns>
        /// The <see cref="Expression"/> that maps the input parameter to the
        /// selector result.
        /// </returns>
        protected abstract Expression BuildSelector(Expression expression);

        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be passed on
        /// to other builders in the workflow.
        /// </summary>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build()
        {
            var source = Arguments.Single();
            var parameterType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(parameterType);
            var selectorBody = BuildSelector(parameter);
            var selector = Expression.Lambda(selectorBody, parameter);
            return Expression.Call(selectMethod.MakeGenericMethod(parameterType, selectorBody.Type), source, selector);
        }
    }
}
