using Bonsai;
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;

namespace $rootnamespace$
{
    [Combinator]
    [Description("")]
    [WorkflowElementCategory(ElementCategory.Sink)]
    public class $safeitemname$
    {
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Do(input =>
            {
                // TODO: compute observable side-effects.
                throw new NotImplementedException();
            });
        }
    }
}
