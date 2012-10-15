using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class Condition<TSource> : Transform<TSource, bool>
    {
    }

    public abstract class Condition<TFirst, TSecond> : Transform<TFirst, TSecond, bool>
    {
    }
}
