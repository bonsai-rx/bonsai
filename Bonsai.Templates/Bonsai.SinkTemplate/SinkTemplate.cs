using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

// TODO: replace this with the sink input type.
using TSource = System.String;

namespace $rootnamespace$
{
    public class $safeitemname$ : Sink<TSource>
    {
        public override IObservable<TSource> Process(IObservable<TSource> source)
        {
            return source.Do(input =>
            {
                // TODO: compute observable side-effects.
                throw new NotImplementedException();
            });
        }
    }
}
