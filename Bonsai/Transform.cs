using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class Transform<TSource, TResult> : LoadableElement
    {
        public abstract TResult Process(TSource input);
    }

    public abstract class Transform<TFirst, TSecond, TResult> : LoadableElement
    {
        internal TResult Process(Tuple<TFirst, TSecond> input)
        {
            return Process(input.Item1, input.Item2);
        }

        public abstract TResult Process(TFirst first, TSecond second);
    }
}
