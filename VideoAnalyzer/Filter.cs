using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VideoAnalyzer
{
    public abstract class Filter<TInput, TOutput> : WorkflowElement
    {
        public abstract TOutput Process(TInput input);
    }
}
