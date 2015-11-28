using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    [Description("Sorts the collection of binary regions by descending order of area.")]
    public class SortBinaryRegions : Transform<ConnectedComponentCollection, ConnectedComponentCollection>
    {
        public override IObservable<ConnectedComponentCollection> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var components = input.OrderByDescending(xs => xs.Area).ToList();
                return new ConnectedComponentCollection(components, input.ImageSize);
            });
        }
    }
}
