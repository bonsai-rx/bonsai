using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [Combinator]
    public abstract class WindowCombinator
    {
        public abstract IObservable<IObservable<TSource>> Process<TSource>(IObservable<TSource> source);
    }
}
