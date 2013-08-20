using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class Transform<TSource, TResult> : Combinator<TSource, TResult>
    {
    }
}
