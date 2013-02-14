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
    public class InspectBuilder : CombinatorBuilder
    {
        Subject<object> subject = new Subject<object>();

        public Type ObservableType { get; private set; }

        public IObservable<object> Output { get; private set; }

        public override Expression Build()
        {
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
                return base.Build();
            }
        }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Do(xs => subject.OnNext(xs));
        }
    }
}
