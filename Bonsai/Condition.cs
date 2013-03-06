using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [Condition]
    public abstract class Condition<TSource> : Transform<TSource, bool>
    {
    }

    [Condition]
    public abstract class Condition<TFirst, TSecond> : Transform<TFirst, TSecond, bool>
    {
    }
}
