using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace $rootnamespace$
{
    /// <summary>
    /// Represents an operator that transforms each value in the sequence.
    /// </summary>
    [Combinator]
    [Description("Transforms each value in the sequence.")]
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class $safeitemname$
    {
        /// <summary>
        /// Transforms each value in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of values to transform.
        /// </param>
        /// <returns>
        /// A sequence of transformed values.
        /// </returns>
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Select(input =>
            {
                // TODO: process the input object and return the result.
                return input;
            });
        }
    }
}
