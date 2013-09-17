using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [Combinator]
    public abstract class BinaryCombinator
    {
        public abstract IObservable<TSource> Process<TSource, TOther>(IObservable<TSource> source, IObservable<TOther> other);
    }

    [Combinator]
    public abstract class BinaryCombinator<TOther>
    {
        public abstract IObservable<TSource> Process<TSource>(IObservable<TSource> source, IObservable<TOther> other);
    }

    [Combinator]
    public abstract class BinaryCombinator<TSource, TOther>
    {
        public abstract IObservable<TSource> Process(IObservable<TSource> source, IObservable<TOther> other);
    }
}
