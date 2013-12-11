using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    [WorkflowElementCategory(ElementCategory.Condition)]
    public abstract class Condition : Combinator
    {
    }

    [Condition]
    [WorkflowElementCategory(ElementCategory.Condition)]
    public abstract class Condition<TSource> : Transform<TSource, bool>
    {
    }
}
