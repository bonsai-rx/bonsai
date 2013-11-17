using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class SortBinaryRegions : Transform<ConnectedComponentCollection, ConnectedComponentCollection>
    {
        public override IObservable<ConnectedComponentCollection> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var components = new List<ConnectedComponent>(input);
                components.Sort((xs, ys) => xs.Area.CompareTo(ys.Area));
                return new ConnectedComponentCollection(components, input.ImageSize);
            });
        }
    }
}
