using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Computes image moments from polygon contours or rasterized shapes to extract binary region properties.")]
    public class BinaryRegionAnalysis : Transform<Contours, ConnectedComponentCollection>
    {
        public IObservable<ConnectedComponent> Process(IObservable<IplImage> source)
        {
            return source.Select(input => ConnectedComponent.FromImage(input));
        }

        public IObservable<ConnectedComponent> Process(IObservable<Seq> source)
        {
            return source.Select(input => ConnectedComponent.FromContour(input));
        }

        public override IObservable<ConnectedComponentCollection> Process(IObservable<Contours> source)
        {
            return source.Select(input =>
            {
                var currentContour = input.FirstContour;
                var output = new ConnectedComponentCollection(input.ImageSize);

                while (currentContour != null)
                {
                    var component = ConnectedComponent.FromContour(currentContour);
                    currentContour = currentContour.HNext;
                    output.Add(component);
                }

                return output;
            });
        }

        public IObservable<ConnectedComponentCollection> Process(IObservable<Tuple<Contours, IplImage>> source)
        {
            return source.Select(input =>
            {
                var image = input.Item2;
                var contours = input.Item1;
                var currentContour = contours.FirstContour;
                var output = new ConnectedComponentCollection(image.Size);

                while (currentContour != null)
                {
                    var component = ConnectedComponent.FromContour(currentContour);
                    component.Patch = image.GetSubRect(component.Contour.Rect);
                    currentContour = currentContour.HNext;
                    output.Add(component);
                }

                return output;
            });
        }
    }
}
