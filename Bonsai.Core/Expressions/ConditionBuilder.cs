using System;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Xml.Serialization;
using Bonsai.Reactive;

namespace Bonsai.Expressions
{
    /// <summary>
    /// This type is obsolete. Please use the <see cref="Condition"/> operator instead.
    /// </summary>
    [Obsolete]
    [ProxyType(typeof(Condition))]
    [XmlType("Condition", Namespace = Constants.XmlNamespace)]
    [Description("Filters the elements of an observable sequence according to a condition specified by the encapsulated workflow.")]
    public class ConditionBuilder : Condition
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionBuilder"/> class.
        /// </summary>
        public ConditionBuilder()
            : this(new ExpressionBuilderGraph())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConditionBuilder"/> class
        /// with the specified expression builder workflow.
        /// </summary>
        /// <param name="workflow">
        /// The expression builder workflow instance that will be used by this builder
        /// to generate the output expression tree.
        /// </param>
        public ConditionBuilder(ExpressionBuilderGraph workflow)
            : base(workflow)
        {
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, Func<IObservable<TSource>, IObservable<bool>> condition)
        {
            return Observable.Defer(() =>
            {
                var filter = false;
                return source.Publish(ps => ps
                    .CombineLatest(condition(ps), (xs, ys) => { filter = ys; return xs; })
                    .Sample(ps)
                    .Where(xs => filter));
            });
        }
    }
}
