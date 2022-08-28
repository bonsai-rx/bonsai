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
    [WorkflowElementCategory(ElementCategory.Transform)]
    public class $safeitemname$
    {
        public IObservable<int> Process(IObservable<int> source)
        {
            return source.Select(input =>
            {
                // TODO: process the input object and return the result.
                throw new NotImplementedException();
                return default(int);
            });
        }
    }
}
