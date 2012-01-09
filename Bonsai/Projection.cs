using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class Projection<TSource, TResult> : LoadableElement
    {
        public abstract TResult Process(TSource input);
    }
}
