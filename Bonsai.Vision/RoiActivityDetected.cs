using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class RoiActivityDetected : Transform<RegionActivityCollection, bool>
    {
        public int? Index { get; set; }

        public double Threshold { get; set; }

        public override IObservable<bool> Process(IObservable<RegionActivityCollection> source)
        {
            return source.Select(input =>
            {
                if (Index.HasValue && Index >= 0 && Index < input.Count)
                {
                    return input[Index.Value].Activity.Val0 > Threshold;
                }
                else return input.Where(region => region.Activity.Val0 > 0).Count() > 0;
            });
        }
    }
}
