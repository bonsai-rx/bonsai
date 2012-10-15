using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;

namespace Bonsai
{
    public enum WorkflowElementType
    {
        Source,
        Condition,
        Transform,
        Sink,
        Combinator
    }
}
