using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("TriggeredWindow", Namespace = Constants.XmlNamespace)]
    [Description("Projects the sequence into non-overlapping windows. A window is closed when the second sequence produces an element.")]
    public class TriggeredWindowBuilder : BinaryCombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            var triggerType = Other.Type.GetGenericArguments()[0];
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", new[] { observableType, triggerType }, Source, Other);
        }

        private IObservable<IObservable<TSource>> Combine<TSource, TTrigger>(IObservable<TSource> source, IObservable<TTrigger> trigger)
        {
            return source.Window(() => trigger);
        }
    }
}
