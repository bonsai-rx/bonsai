using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    [XmlType("RemoveTimestamp", Namespace = Constants.XmlNamespace)]
    [Description("Removes timestamp information from the elements of the sequence.")]
    public class RemoveTimestampBuilder : CombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments();
            var timestampedValueType = observableType[0].GetGenericArguments();
            return Expression.Call(typeof(RemoveTimestampBuilder), "Combine", timestampedValueType, Source);
        }

        private static IObservable<TSource> Combine<TSource>(IObservable<Timestamped<TSource>> source)
        {
            return source.Select(xs => xs.Value);
        }
    }
}
