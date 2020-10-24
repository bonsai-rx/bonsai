using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that creates a lookup from an observable
    /// sequence according to the specified key, and optional element selector function.
    /// </summary>
    [DefaultProperty(nameof(KeySelector))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Creats a lookup from an observable sequence according to the specified key.")]
    public class ToLookup : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a string used to specify a key for each element of the observable sequence.
        /// </summary>
        [Description("The inner properties that will be used as key for each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string KeySelector { get; set; }

        /// <summary>
        /// Gets or sets a string used to specify the properties used as elements of the lookup.
        /// </summary>
        [Description("The inner properties that will be used as elements of the lookup.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string ElementSelector { get; set; }

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
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var parameter = Expression.Parameter(parameterType);
            var keySelectorBody = MemberSelector(parameter, KeySelector);
            var keySelectorLambda = Expression.Lambda(keySelectorBody, parameter);
            var elementSelector = ElementSelector;
            var combinator = Expression.Constant(this);
            if (!string.IsNullOrEmpty(elementSelector))
            {
                var elementSelectorBody = MemberSelector(parameter, ElementSelector);
                var elementSelectorLambda = Expression.Lambda(elementSelectorBody, parameter);
                return Expression.Call(
                    combinator,
                    nameof(Process),
                    new[] { parameter.Type, keySelectorLambda.ReturnType, elementSelectorLambda.ReturnType },
                    source,
                    keySelectorLambda,
                    elementSelectorLambda);
            }
            else return Expression.Call(
                combinator,
                nameof(Process),
                new[] { parameter.Type, keySelectorLambda.ReturnType },
                source,
                keySelectorLambda);
        }

        IObservable<ILookup<TKey, TSource>> Process<TSource, TKey>(
            IObservable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.ToLookup(keySelector);
        }

        IObservable<ILookup<TKey, TElement>> Process<TSource, TKey, TElement>(
            IObservable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return source.ToLookup(keySelector, elementSelector);
        }
    }
}
