using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Reactive.Subjects;
using System.Xml.Serialization;
using System.ComponentModel;

namespace Bonsai.Expressions
{
    public class PublishBuilder : CombinatorBuilder
    {
        ObservableHandle handle = new ObservableHandle();

        public LoadableElement PublishHandle
        {
            get { return handle; }
        }

        protected override IObservable<TSource> Combine<TSource>(IObservable<TSource> source)
        {
            handle.ObservableCache = handle.ObservableCache ?? source.Publish().RefCount();
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
