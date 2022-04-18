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
    /// Represents an operator that subsequently sorts the elements of all the
    /// ordered collections in an observable sequence in descending order according
    /// to the specified key.
    /// </summary>
    [DefaultProperty(nameof(KeySelector))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Subsequently sorts the elements of all the ordered collections in an observable sequence in descending order according to the specified key.")]
    public class ThenByDescending : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a value specifying the inner properties used as a key for
        /// further sorting the elements in the ordered collection.
        /// </summary>
        [Description("Specifies the inner properties used as a key for further sorting the elements in the ordered collection.")]
        [Editor("Bonsai.Design.OrderedEnumerableMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string KeySelector { get; set; }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var enumerableType = GetParameterBindings(typeof(IOrderedEnumerable<>), parameterType).FirstOrDefault();
            if (enumerableType == null)
            {
                throw new InvalidOperationException("The elements of the input observable sequence must be of an ordered enumerable type.");
            }

            var parameter = Expression.Parameter(enumerableType.Item1);
            var keySelectorBody = MemberSelector(parameter, KeySelector);
            var keySelectorLambda = Expression.Lambda(keySelectorBody, parameter);
            var combinator = Expression.Constant(this);
            return Expression.Call(
                combinator,
                nameof(Process),
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
