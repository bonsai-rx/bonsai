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
        ObservableHandle handle = new ObservableHandle();
        ReplaySubject<IObservable<object>> subject = new ReplaySubject<IObservable<object>>(1);

        public Type ObservableType { get; private set; }

        public IObservable<IObservable<object>> Output { get; private set; }

        public LoadableElement PublishHandle
        {
            get { return handle; }
        }

        public override Expression Build()
        {
            ObservableType = Source.Type.GetGenericArguments()[0];

            // If source is a publish node, unwrap it to check for inspect nodes
            var methodCall = Source as MethodCallExpression;
            if (methodCall != null && methodCall.Object != null && methodCall.Object.Type == typeof(PublishBuilder))
            {
                methodCall = methodCall.Arguments[0] as MethodCallExpression;
            }

            // If source is already an inspect node, use it
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
            if (handle.ObservableCache == null)
            {
                var sourceInspector = new Subject<object>();
                subject.OnNext(sourceInspector);
                handle.ObservableCache = source.Do(xs => sourceInspector.OnNext(xs), () => sourceInspector.OnCompleted());
            }
            return (IObservable<TSource>)handle.ObservableCache;
        }

        class ObservableHandle : LoadableElement
        {
            public object ObservableCache { get; set; }

            protected override void Unload()
            {
                ObservableCache = null;
                base.Unload();
            }
        }
    }
}
