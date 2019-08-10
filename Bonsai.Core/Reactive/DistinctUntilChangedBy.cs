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
    /// Represents a combinator that ensures that only distinct contiguous
    /// elements according to the specified key are propagated.
    /// </summary>
    [DefaultProperty("KeySelector")]
    [XmlType(Namespace = Constants.XmlNamespace)]
    [Description("Ensures that only distinct contiguous elements according to the specified key are propagated.")]
    public class DistinctUntilChangedBy : SingleArgumentExpressionBuilder
    {
        /// <summary>
        /// Gets or sets a string used to specify a key to test for contiguity of each element in the observable sequence.
        /// </summary>
        [Description("The inner properties that will be used to test the contiguity of each element in the sequence.")]
        [Editor("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
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
            var parameter = Expression.Parameter(parameterType);
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

        IObservable<TSource> Process<TSource, TKey>(
            IObservable<TSource> source,
            Func<TSource, TKey> keySelector)
        {
            return source.DistinctUntilChanged(keySelector);
        }
    }

    /// <summary>
    /// This type is obsolete. Please use the <see cref="DistinctUntilChangedBy"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(DistinctUntilChangedBy))]
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class DistinctUntilChangedByBuilder : DistinctUntilChangedBy
    {
    }
}
