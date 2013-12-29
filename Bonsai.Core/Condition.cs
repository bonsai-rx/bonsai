using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Represents a generic operation on observable sequences which filters out
    /// elements from the sequence without modifying the elements themselves.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Condition)]
    public abstract class Condition : Combinator
    {
    }

    /// <summary>
    /// Represents an operation that maps the elements of an observable sequence into
    /// <see cref="Boolean"/> values.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    [Condition]
    public abstract class Condition<TSource> : Transform<TSource, bool>
    {
    }
}
