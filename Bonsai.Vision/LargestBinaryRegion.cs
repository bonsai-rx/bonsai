using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Returns the largest binary region from the input collection of connected components.")]
    public class LargestBinaryRegion : Transform<ConnectedComponentCollection, ConnectedComponent>
    {
        public override IObservable<ConnectedComponent> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var largest = new ConnectedComponent();
                if (input.Count > 0)
                {
                    for (int i = 0; i < input.Count; i++)
                    {
                        var component = input[i];
                        if (component.Area > largest.Area)
                        {
                            largest = component;
                        }
                    }
                }
                else
                {
                    largest.Centroid = new Point2f(float.NaN, float.NaN);
                    largest.Orientation = double.NaN;
                }

                return largest;
            });
        }
    }
}
