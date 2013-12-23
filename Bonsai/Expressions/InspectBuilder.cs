using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;
using System.Reactive.Concurrency;

namespace Bonsai.Expressions
{
    public class InspectBuilder : ExpressionBuilder, INamedElement
    {
        public InspectBuilder(ExpressionBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            Builder = builder;
        }

        public ExpressionBuilder Builder { get; private set; }

        public Type ObservableType { get; private set; }

        public IObservable<IObservable<object>> Output { get; private set; }

        public override Range<int> ArgumentRange
        {
            get { return Builder.ArgumentRange; }
        }

        public string Name
        {
            get { return GetElementDisplayName(Builder); }
        }

        public override Expression Build()
        {
            foreach (var argument in Arguments)
            {
                Builder.Arguments.Add(argument);
            }

            try
            {
                var source = Builder.Build();
                var subject = new ReplaySubject<IObservable<object>>(1, Scheduler.Immediate);
                ObservableType = source.Type.GetGenericArguments()[0];

                // If source is already an inspect node, use it
                var methodCall = source as MethodCallExpression;
                if (methodCall != null && methodCall.Object != null && methodCall.Object.Type == typeof(InspectBuilder))
                {
                    var inspectBuilder = (InspectBuilder)((ConstantExpression)methodCall.Object).Value;
                    Output = inspectBuilder.Output;
                    return source;
                }
                else
                {
                    Output = subject;
                    var subjectExpression = Expression.Constant(subject);
                    return Expression.Call(typeof(InspectBuilder), "Process", new[] { ObservableType }, source, subjectExpression);
                }
            }
            finally { Builder.Arguments.Clear(); }
        }

        static IObservable<TSource> Process<TSource>(IObservable<TSource> source, ReplaySubject<IObservable<object>> subject)
        {
            return Observable.Defer(() =>
            {
                var sourceInspector = new ReplaySubject<object>(1, Scheduler.Immediate);
                subject.OnNext(sourceInspector);
                return source.Do(
                    xs => sourceInspector.OnNext(xs),
                    ex => sourceInspector.OnError(ex),
                    () => sourceInspector.OnCompleted());
            });
        }
    }
}
