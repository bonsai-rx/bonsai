using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [Condition]
    [Selector]
    public abstract class Condition<TSource> : LoadableElement
    {
        public abstract bool Process(TSource input);
    }

    [Condition]
    [Selector]
    public abstract class Condition<TFirst, TSecond> : LoadableElement
    {
        internal bool Process(Tuple<TFirst, TSecond> input)
        {
            return Process(input.Item1, input.Item2);
        }

        public abstract bool Process(TFirst first, TSecond second);
    }
}
