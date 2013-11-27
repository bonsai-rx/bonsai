using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

// TODO: replace this with the condition input type.
using TSource = System.String;

namespace $rootnamespace$
{
    public class $safeitemname$ : Condition<TSource>
    {
        public override IObservable<bool> Process(IObservable<TSource> source)
        {
            return source.Select(input =>
            {
                // TODO: compute observable side-effects.
                throw new NotImplementedException();
            });
        }
    }
}
