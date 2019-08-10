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
    [Description("Sorts the elements of the input enumerable sequences in ascending order according to the specified key.")]
    public class OrderBy : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a string used to specify a key for each element of the input enumerable sequences.
        /// </summary>
        [Description("The inner properties that will be used as a key for sorting the elements of the enumerable sequences.")]
        [Editor("Bonsai.Design.EnumerableMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
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
            var enumerableType = GetParameterBindings(typeof(IEnumerable<>), parameterType).FirstOrDefault();
            if (enumerableType == null)
            {
                throw new InvalidOperationException("The elements of the input observable sequence must be an enumerable type.");
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

        IObservable<IOrderedEnumerable<TSource>> Process<TSource, TKey>(IObservable<IEnumerable<TSource>> source, Func<TSource, TKey> keySelector)
        {
            return source.Select(input => input.OrderBy(keySelector));
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="OrderBy"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(OrderBy))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class OrderByBuilder : OrderBy
    {
    }
}
