using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.Reflection;
using System.ComponentModel;
using System.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// Represents an expression builder that defines a simple selector on the elements of
    /// an observable sequence by converting each element into the default <see cref="Unit"/> instance.
    /// </summary>
    [XmlType("Unit", Namespace = Constants.XmlNamespace)]
    [Description("Converts a sequence of any type into a sequence of Unit type elements.")]
    public class UnitBuilder : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Generates an <see cref="Expression"/> node that will be passed on
        /// to other builders in the workflow.
        /// </summary>
        /// <returns>An <see cref="Expression"/> tree node.</returns>
        public override Expression Build()
        {
            var source = Arguments.Single();
            return Expression.Call(typeof(UnitBuilder), "Process", source.Type.GetGenericArguments(), source);
        }

        static IObservable<Unit> Process<TSource>(IObservable<TSource> source)
        {
            return source.Select(xs => Unit.Default);
        }
    }
}
