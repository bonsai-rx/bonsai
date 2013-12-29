using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai
{
    /// <summary>
    /// Represents a generic operation which introduces side effects on an observable
    /// sequence without modifying its elements.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class Sink : Combinator
    {
    }

    /// <summary>
    /// Represents an operation which introduces side effects on observable sequences
    /// of a specific type without modifying its elements.
    /// </summary>
    /// <typeparam name="TSource">The type of the elements in the source sequence.</typeparam>
    [WorkflowElementCategory(ElementCategory.Sink)]
    public abstract class Sink<TSource> : Combinator<TSource, TSource>
    {
    }
}
