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

        public IObservable<object> Output
        {
            get { return subject; }
        }

        public override Expression Build()
        {
            ObservableType = Source.Type.GetGenericArguments()[0];
            return base.Build();
        }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            return source.Do(xs => subject.OnNext(xs));
        }
    }
}
