using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class AverageBinaryRegion : Projection<ConnectedComponentCollection, ConnectedComponentCollection>
    {
        public override ConnectedComponentCollection Process(ConnectedComponentCollection input)
        {
            var result = new ConnectedComponentCollection(input.ImageSize);

            CvPoint centroid = new CvPoint();
            double angle = 0;
            double area = 0;
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
                var component = new ConnectedComponent();
                centroid.X = (int)(centroid.X / (double)input.Count);
                centroid.Y = (int)(centroid.Y / (double)input.Count);
                component.Centroid = centroid;
                component.Orientation = angle / input.Count;
                component.Area = area;
                component.Contour = CvContour.FromCvSeq(CvSeq.Null);
                result.Add(component);
            }

            return result;
        }
    }
}
