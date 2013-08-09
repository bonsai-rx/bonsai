using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;

namespace Bonsai.Expressions
{
    public class InspectBuilder : CombinatorExpressionBuilder
    {
        ReplaySubject<IObservable<object>> subject;

        public Type ObservableType { get; private set; }

        public IObservable<IObservable<object>> Output { get; private set; }

        public override Expression Build()
        {
            subject = new ReplaySubject<IObservable<object>>(1);
            ObservableType = Source.Type.GetGenericArguments()[0];

            // If source is already an inspect node, use it
            var methodCall = Source as MethodCallExpression;
            if (methodCall != null && methodCall.Object != null && methodCall.Object.Type == typeof(InspectBuilder))
            {
                var inspectBuilder = (InspectBuilder)((ConstantExpression)methodCall.Object).Value;
                Output = inspectBuilder.Output;
                return Source;
            }
            else
            {
                Output = subject;
                var combinatorExpression = Expression.Constant(this);
                return Expression.Call(combinatorExpression, "Process", new[] { ObservableType }, Source);
            }
        }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            var sourceInspector = new ReplaySubject<object>(1);
            subject.OnNext(sourceInspector);
            return source.Do(
                xs => sourceInspector.OnNext(xs),
                ex => sourceInspector.OnError(ex),
                () => sourceInspector.OnCompleted());
        }
    }
}
