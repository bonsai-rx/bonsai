using Bonsai;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

// TODO: replace this with the source output type.
using TSource = System.String;

namespace Bonsai.ItemTemplates
{
    public class SourceTemplate : Source<TSource>
    {
        protected override IObservable<TSource> Generate()
        {
            // TODO: generate the observable sequence.
            throw new NotImplementedException();
        }
    }
}
