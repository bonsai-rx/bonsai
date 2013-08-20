using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [Predicate]
    public abstract class Predicate<TSource> : Selector<TSource, bool>
    {
    }

    [Predicate]
    public abstract class Predicate<TFirst, TSecond> : Selector<TFirst, TSecond, bool>
    {
    }
}
