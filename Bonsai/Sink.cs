using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class Sink : Combinator
    {
    }

    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class Sink<TSource> : Combinator<TSource, TSource>
    {
    }
}
