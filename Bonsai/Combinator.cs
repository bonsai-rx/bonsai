using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class Combinator<TSource, TResult> : LoadableElement
    {
        public abstract IObservable<TResult> Process(IObservable<TSource> source);
    }
}
