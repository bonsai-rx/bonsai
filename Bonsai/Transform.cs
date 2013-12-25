using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Represents an operation on observable sequences which transforms every element of the source
    /// sequence into an element in the result sequence.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    /// <typeparam name="TResult">The type of the elements in the result sequence.</typeparam>
    [WorkflowElementCategory(ElementCategory.Transform)]
    public abstract class Transform<TSource, TResult> : Combinator<TSource, TResult>
    {
    }
}
