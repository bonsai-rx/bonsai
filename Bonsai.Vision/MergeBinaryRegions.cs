using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    [Description("Merges two connected component collections into a single collection.")]
    public class MergeBinaryRegions : Transform<Tuple<ConnectedComponentCollection, ConnectedComponentCollection>, ConnectedComponentCollection>
    {
        public override IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<ConnectedComponentCollection, ConnectedComponentCollection>> source)
        {
            return source.Select(input =>
            {
                var first = input.Item1;
                var second = input.Item2;
                var output = new ConnectedComponentCollection(first.Concat(second).ToList(), first.ImageSize);
                return output;
            });
        }
    }
}
