using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class Filter<TSource> : Projection<TSource, bool>
    {
    }
}
