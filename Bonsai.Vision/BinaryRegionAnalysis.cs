using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Computes image moments of connected component contours to extract binary region properties.")]
    public class BinaryRegionAnalysis : Transform<Contours, ConnectedComponentCollection>
    {
        public override ConnectedComponentCollection Process(Contours input)
        {
            var currentContour = input.FirstContour;
            var output = new ConnectedComponentCollection(input.ImageSize);

            while (currentContour != null && !currentContour.IsInvalid)
            {
                var contour = ConnectedComponent.FromContour(currentContour);
                currentContour = currentContour.HNext;
                output.Add(contour);
            }

            return output;
        }
    }
}
