using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Vision
{
    public class RoiActivityDetected : Filter<RegionActivityCollection>
    {
        public int? Index { get; set; }

        public override bool Process(RegionActivityCollection input)
        {
            if (Index.HasValue && Index >= 0 && Index < input.Count)
            {
                return input[Index.Value].Activity.Val0 > 0;
            }
            else return input.Where(region => region.Activity.Val0 > 0).Count() > 0;
        }
    }
}
