using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Computes the average binary region from the input collection of connected components.")]
    public class AverageBinaryRegion : Transform<ConnectedComponentCollection, ConnectedComponent>
    {
        public override IObservable<ConnectedComponent> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var result = new ConnectedComponent();

                double angle = 0;
                double area = 0;
                Point2f centroid = new Point2f();
                for (int i = 0; i < input.Count; i++)
                {
                    var component = input[i];
                    centroid.X += component.Centroid.X;
                    centroid.Y += component.Centroid.Y;
                    angle += component.Orientation;
                    area += component.Area;
                }

                if (input.Count > 0)
                {
                    centroid.X = centroid.X / input.Count;
                    centroid.Y = centroid.Y / input.Count;
                    result.Centroid = centroid;
                    result.Orientation = angle / input.Count;
                    result.Area = area;
                    result.Contour = Contour.FromSeq(null);
                }
                else
                {
                    result.Centroid = new Point2f(float.NaN, float.NaN);
                    result.Orientation = double.NaN;
                }

                return result;
            });
        }
    }
}
