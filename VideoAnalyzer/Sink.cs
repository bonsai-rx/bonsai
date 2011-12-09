using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoAnalyzer
{
    public abstract class Sink<T> : WorkflowElement
    {
        public abstract void Consume(T input);
    }
}
