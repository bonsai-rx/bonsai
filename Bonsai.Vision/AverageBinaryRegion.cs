using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class AverageBinaryRegion : Selector<ConnectedComponentCollection, ConnectedComponent>
    {
        public override ConnectedComponent Process(ConnectedComponentCollection input)
        {
            var result = new ConnectedComponent();

            double angle = 0;
            double area = 0;
            CvPoint2D32f centroid = new CvPoint2D32f();
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
                result.Contour = CvContour.FromCvSeq(CvSeq.Null);
            }

            return result;
        }
    }
}
