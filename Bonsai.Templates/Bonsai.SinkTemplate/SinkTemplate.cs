using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace $rootnamespace$
{
    /// <summary>
    /// Represents an operator that performs an action on each value in the sequence.
    /// </summary>
    [Combinator]
    [Description("Performs an action on each value in the sequence.")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class $safeitemname$
    {
        /// <summary>
        /// Performs an action on each value in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values on which to perform the action.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/> sequence
        /// but where there is an additional side effect for each value.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Do(input =>
            {
                // TODO: compute observable side-effects.
            });
        }
    }
}
