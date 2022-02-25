using System;
using OpenCV.Net;
using System.Xml.Serialization;

namespace Bonsai.Vision
{
    public class ConnectedComponent
    {
        public Point2f Centroid { get; set; }

        public double Orientation { get; set; }

        public double MajorAxisLength { get; set; }

        public double MinorAxisLength { get; set; }

        public double Area { get; set; }

        [XmlIgnore]
        public IplImage Patch { get; set; }

        [XmlIgnore]
        public Contour Contour { get; set; }

        public static ConnectedComponent FromImage(IplImage image, bool binary = false)
        {
            var moments = new Moments(image, binary);
            var component = FromMoments(moments);
            component.Patch = image;
            return component;
        }

        public static ConnectedComponent FromContour(Seq currentContour)
        {
            var moments = new Moments(currentContour);
            var component = FromMoments(moments);
            component.Contour = Contour.FromSeq(currentContour);
            return component;
        }

        public static ConnectedComponent FromMoments(Moments moments)
        {
            var component = new ConnectedComponent();
            component.Area = moments.M00;

            // Cemtral moments can only be computed for components with non-zero area
            if (moments.M00 > 0)
            {
                // Compute centroid components
                var x = moments.M10 / moments.M00;
                var y = moments.M01 / moments.M00;
                component.Centroid = new Point2f((float)x, (float)y);

                // Compute covariance matrix of image intensity
                var miu20 = moments.Mu20 / moments.M00;
                var miu02 = moments.Mu02 / moments.M00;
                var miu11 = moments.Mu11 / moments.M00;

                // Compute orientation and major/minor axis length
                var b = 2 * miu11;
                var a = miu20 - miu02;
                var deviation = Math.Sqrt(b * b + a * a);
                component.Orientation = 0.5 * Math.Atan2(b, a);
                component.MajorAxisLength = 2.75 * Math.Sqrt(miu20 + miu02 + deviation);
                component.MinorAxisLength = 2.75 * Math.Sqrt(miu20 + miu02 - deviation);
            }
            else
            {
                component.Centroid = new Point2f(float.NaN, float.NaN);
                component.Orientation = double.NaN;
            }

            return component;
        }
    }
}
