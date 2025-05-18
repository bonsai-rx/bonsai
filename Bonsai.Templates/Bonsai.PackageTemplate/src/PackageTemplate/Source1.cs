using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace $projectname$
{
    /// <summary>
    /// Represents an operator that produces a sequence of values.
    /// </summary>
    [Description("Produces a sequence of values.")]
    [Combinator(MethodName = nameof(Generate))]
    [WorkflowElementCategory(ElementCategory.Source)]
    public class Source1
    {
        /// <summary>
        /// Returns an observable sequence that produces a single value.
        /// </summary>
        /// <returns>
        /// A sequence with a single value.
        /// </returns>
        public IObservable<int> Generate()
        {
            return Observable.Return(0);
        }
    }
}
