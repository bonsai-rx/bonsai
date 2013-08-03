using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [BinaryCombinator]
    public abstract class BinaryCombinator : LoadableElement
    {
        public abstract IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other);
    }

    [BinaryCombinator]
    public abstract class BinaryCombinator<TOther> : LoadableElement
    {
        public abstract IObservable<TSource> Process<TSource>(IObservable<TSource> source, IObservable<TOther> other);
    }

    [BinaryCombinator]
    public abstract class BinaryCombinator<TSource, TOther> : LoadableElement
    {
        public abstract IObservable<TSource> Process(IObservable<TSource> source, IObservable<TOther> other);
    }
}
