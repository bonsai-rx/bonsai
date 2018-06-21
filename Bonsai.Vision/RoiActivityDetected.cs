using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;

namespace Bonsai.Vision
{
    [Description("Checks whether the specified region of interest reported activity above a given threshold.")]
    public class RoiActivityDetected : Transform<RegionActivityCollection, bool>
    {
        [Description("The index of the region of interest to test.")]
        public int? Index { get; set; }

        [Description("The activity detection threshold.")]
        public double Threshold { get; set; }

        public override IObservable<bool> Process(IObservable<RegionActivityCollection> source)
        {
            return source.Select(input =>
            {
                if (Index.HasValue && Index >= 0 && Index < input.Count)
                {
                    return input[Index.Value].Activity.Val0 > Threshold;
                }
                else return input.Where(region => region.Activity.Val0 > Threshold).Count() > 0;
            });
        }
    }
}
