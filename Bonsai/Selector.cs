using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [Selector]
    public abstract class Selector<TSource, TResult> : LoadableElement
    {
        public abstract TResult Process(TSource input);
    }

    [Selector]
    public abstract class Selector<TFirst, TSecond, TResult> : LoadableElement
    {
        internal TResult Process(Tuple<TFirst, TSecond> input)
        {
            return Process(input.Item1, input.Item2);
        }

        public abstract TResult Process(TFirst first, TSecond second);
    }
}
