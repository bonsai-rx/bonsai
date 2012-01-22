using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Disposables;

namespace Bonsai
{
    public abstract class DynamicSink : LoadableElement
    {
        LoadableElement sink;

        protected abstract Sink<T> CreateProcessor<T>();

        public Action<T> Create<T>()
        {
            var processor = CreateProcessor<T>();
            sink = processor;
            return processor.Process;
        }

        public override IDisposable Load()
        {
            return new CompositeDisposable(sink.Load(), base.Load());
        }
    }
}
