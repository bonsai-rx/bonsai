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
    /// Represents a combinator that sorts the elements of the input enumerable
    /// sequences according to the specified key.
    /// </summary>
    [DefaultProperty("KeySelector")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Performs a subsequent ordering of the elements of the input enumerable sequences, in descending order.")]
    public class ThenByDescending : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a string used to specify a key for each element of the input enumerable sequences.
        /// </summary>
        [Description("The inner properties that will be used as a key for sorting the elements of the enumerable sequences.")]
        [Editor("Bonsai.Design.OrderedEnumerableMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string KeySelector { get; set; }

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
            var enumerableType = GetParameterBindings(typeof(IOrderedEnumerable<>), parameterType).FirstOrDefault();
            if (enumerableType == null)
            {
                throw new InvalidOperationException("The elements of the input observable sequence must be an ordered enumerable type.");
            }

            var parameter = Expression.Parameter(enumerableType.Item1);
            var keySelectorBody = MemberSelector(parameter, KeySelector);
            var keySelectorLambda = Expression.Lambda(keySelectorBody, parameter);
            var combinator = Expression.Constant(this);
            return Expression.Call(
                combinator,
                "Process",
                new[] { parameter.Type, keySelectorLambda.ReturnType },
                source,
                keySelectorLambda);
        }

        IObservable<IOrderedEnumerable<TSource>> Process<TSource, TKey>(IObservable<IOrderedEnumerable<TSource>> source, Func<TSource, TKey> keySelector)
        {
            return source.Select(input => input.ThenByDescending(keySelector));
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="ThenByDescending"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(ThenByDescending))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ThenByDescendingBuilder : ThenByDescending
    {
    }
}
