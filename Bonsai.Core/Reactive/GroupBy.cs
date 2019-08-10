using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Reactive
{
    /// <summary>
    /// Represents a combinator that groups the elements of an observable
    /// sequence according to the specified key.
    /// </summary>
    [DefaultProperty("KeySelector")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Groups the elements of an observable sequence according to the specified key.")]
    public class GroupBy : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a string used to specify a key for each element of the observable sequence.
        /// </summary>
        [Description("The inner properties that will be used as key for each element of the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string KeySelector { get; set; }

        /// <summary>
        /// Gets or sets a string used to specify the properties used as elements in each
        /// grouped observable sequence.
        /// </summary>
        [Description("The inner properties that will be used as elements in each grouped sequence.")]
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
                    "Process",
                    new[] { parameter.Type, keySelectorLambda.ReturnType, elementSelectorLambda.ReturnType },
                    source,
                    keySelectorLambda,
                    elementSelectorLambda);
            }
            else return Expression.Call(
                combinator,
                "Process",
                new[] { parameter.Type, keySelectorLambda.ReturnType },
                source,
                keySelectorLambda);
        }

        IObservable<IGroupedObservable<TKey, TSource>> Process<TSource, TKey>(
            IObservable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.GroupBy(keySelector);
        }

        IObservable<IGroupedObservable<TKey, TElement>> Process<TSource, TKey, TElement>(
            IObservable<TSource> source,
            Func<TSource, TKey> keySelector,
            Func<TSource, TElement> elementSelector)
        {
            return source.GroupBy(keySelector, elementSelector);
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="GroupBy"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(GroupBy))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class GroupByBuilder : GroupBy
    {
    }
}
