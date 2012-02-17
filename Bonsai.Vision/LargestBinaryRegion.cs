using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class LargestBinaryRegion : Projection<ConnectedComponentCollection, ConnectedComponentCollection>
    {
        public override ConnectedComponentCollection Process(ConnectedComponentCollection input)
        {
            var result = new ConnectedComponentCollection(input.ImageSize);

            CvPoint centroid = new CvPoint();
            double angle = 0;
            double area = 0;
            CvContour contour = null;
            for (int i = 0; i < input.Count; i++)
            {
                var component = input[i];
                if (area == 0 || component.Area > area)
                {
                    centroid.X = component.Center.X;
                    centroid.Y = component.Center.Y;
                    angle = component.Angle;
                    area = component.Area;
                    contour = component.Contour;
                }
            }

            if (input.Count > 0)
            {
                var component = new ConnectedComponent();
                component.Center = centroid;
                component.Angle = angle;
                component.Area = area;
                component.Contour = contour;
                result.Add(component);
            }

            return result;
        }
    }
}
