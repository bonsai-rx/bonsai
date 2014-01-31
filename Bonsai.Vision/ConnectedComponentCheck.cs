using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class ConnectedComponentCheck : Transform<ConnectedComponentCollection, bool>
    {
        public override IObservable<bool> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input => input.Count > 0);
        }
    }
}
