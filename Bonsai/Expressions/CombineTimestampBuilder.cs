using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Expressions
{
    [XmlType("CombineTimestamp", Namespace = Constants.XmlNamespace)]
    public class CombineTimestampBuilder : CombinatorExpressionBuilder
    {
        public override Expression Build()
        {
            var observableType = Source.Type.GetGenericArguments()[0];
            if (!observableType.IsGenericType || observableType.GetGenericTypeDefinition() != typeof(Tuple<,>))
            {
                throw new InvalidOperationException("Combine timestamp requires a System.Tuple pair of data type and System.DateTimeOffset.");
            }

            var dataType = observableType.GetGenericArguments()[0];
            var combinatorExpression = Expression.Constant(this);
            return Expression.Call(combinatorExpression, "Combine", new[] { dataType }, Source);
        }

        IObservable<Timestamped<T>> Combine<T>(IObservable<Tuple<T, DateTimeOffset>> source)
        {
            return source.Select(xs => new Timestamped<T>(xs.Item1, xs.Item2));
        }
    }
}
