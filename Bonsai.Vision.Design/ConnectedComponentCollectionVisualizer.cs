using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using Bonsai.Vision;

[assembly: TypeVisualizer(typeof(ConnectedComponentCollectionVisualizer), Target = typeof(ConnectedComponentCollection))]

namespace Bonsai.Vision.Design
{
    public class ConnectedComponentCollectionVisualizer : IplImageVisualizer
    {
        public override void Show(object value)
        {
            var connectedComponents = (ConnectedComponentCollection)value;
            var output = new IplImage(connectedComponents.ImageSize, 8, 3);
            output.SetZero();

            foreach (var component in connectedComponents)
            {
                var centroid = component.Centroid;
                var orientation = component.Orientation;
                var minorAxisOrientation = orientation + Math.PI / 2.0;
                var halfMajorAxis = component.MajorAxisLength * 0.5;
                var halfMinorAxis = component.MinorAxisLength * 0.5;
                var major1 = new CvPoint((int)(centroid.X + halfMajorAxis * Math.Cos(orientation)), (int)(centroid.Y + halfMajorAxis * Math.Sin(orientation)));
                var major2 = new CvPoint((int)(centroid.X - halfMajorAxis * Math.Cos(orientation)), (int)(centroid.Y - halfMajorAxis * Math.Sin(orientation)));
                var minor1 = new CvPoint((int)(centroid.X + halfMinorAxis * Math.Cos(minorAxisOrientation)), (int)(centroid.Y + halfMinorAxis * Math.Sin(minorAxisOrientation)));
                var minor2 = new CvPoint((int)(centroid.X - halfMinorAxis * Math.Cos(minorAxisOrientation)), (int)(centroid.Y - halfMinorAxis * Math.Sin(minorAxisOrientation)));

                Core.cvDrawContours(output, component.Contour, CvScalar.All(255), CvScalar.All(0), 0, -1, 8, CvPoint.Zero);
                Core.cvDrawContours(output, component.Contour, CvScalar.Rgb(255, 0, 0), CvScalar.Rgb(0, 0, 255), 0, 1, 8, CvPoint.Zero);
                Core.cvLine(output, major1, major2, CvScalar.Rgb(0, 0, 255), 1, 8, 0);
                Core.cvLine(output, minor1, minor2, CvScalar.Rgb(255, 0, 0), 1, 8, 0);
                Core.cvCircle(output, new CvPoint(centroid), 2, CvScalar.Rgb(255, 0, 0), -1, 8, 0);
            }

            base.Show(output);
        }
    }
}
