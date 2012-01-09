using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    public abstract class Sink<T> : LoadableElement
    {
        public abstract void Process(T input);
    }
}
