using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Computes image moments of connected component contours to extract binary region properties.")]
    public class BinaryRegionAnalysis : Transform<Contours, ConnectedComponentCollection>
    {
        public override IObservable<ConnectedComponentCollection> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                var currentContour = input.FirstContour;
                var output = new ConnectedComponentCollection(input.ImageSize);

                while (currentContour != null)
                {
                    var contour = ConnectedComponent.FromContour(currentContour);
                    currentContour = currentContour.HNext;
                    output.Add(contour);
                }

                return output;
            });
        }
    }
}
