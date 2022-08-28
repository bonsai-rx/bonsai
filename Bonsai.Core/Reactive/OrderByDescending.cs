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
    /// Represents an operator that sorts the elements of all the collections
    /// in an observable sequence in descending order according to the specified key.
    /// </summary>
    [DefaultProperty(nameof(KeySelector))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Sorts the elements of all the collections in the sequence in descending order according to the specified key.")]
    public class OrderByDescending : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a value specifying the inner properties used as a key for
        /// sorting the elements in the collection.
        /// </summary>
        [Description("Specifies the inner properties used as a key for sorting the elements in the collection.")]
        [Editor("Bonsai.Design.EnumerableMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string KeySelector { get; set; }

        /// <inheritdoc/>
        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var parameterType = source.Type.GetGenericArguments()[0];
            var enumerableType = GetParameterBindings(typeof(IEnumerable<>), parameterType).FirstOrDefault();
            if (enumerableType == null)
            {
                throw new InvalidOperationException("The elements of the input observable sequence must be of an enumerable type.");
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

        IObservable<IOrderedEnumerable<TSource>> Process<TSource, TKey>(IObservable<IEnumerable<TSource>> source, Func<TSource, TKey> keySelector)
        {
            return source.Select(input => input.OrderByDescending(keySelector));
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="OrderByDescending"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(OrderByDescending))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class OrderByDescendingBuilder : OrderByDescending
    {
    }
}
