using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Xml.Serialization;

namespace Bonsai.Vision
{
    public class ConnectedComponent
    {
        public CvPoint2D32f Centroid { get; set; }

        public double Orientation { get; set; }

        public double MajorAxisLength { get; set; }

        public double MinorAxisLength { get; set; }

        public double Area { get; set; }

        [XmlIgnore]
        public CvContour Contour { get; set; }

        public static ConnectedComponent FromContour(CvSeq currentContour)
        {
            CvMoments moments;
            ImgProc.cvMoments(currentContour, out moments, 0);

            var component = new ConnectedComponent();
            component.Area = moments.m00;
            component.Contour = CvContour.FromCvSeq(currentContour);

            // Cemtral moments can only be computed for components with non-zero area
            if (moments.m00 > 0)
            {
                // Compute centroid components
                var x = moments.m10 / moments.m00;
                var y = moments.m01 / moments.m00;
                component.Centroid = new CvPoint2D32f((float)x, (float)y);

                // Compute second-order central moments
                var miu20 = moments.m20 / moments.m00 - x * x;
                var miu02 = moments.m02 / moments.m00 - y * y;
                var miu11 = moments.m11 / moments.m00 - x * y;

                // Compute orientation and major/minor axis length
                var b = 2 * miu11;
                component.Orientation = 0.5 * Math.Atan2(b, miu20 - miu02);
                var deviation = Math.Sqrt(b * b + Math.Pow(miu20 - miu02, 2));
                component.MajorAxisLength = Math.Sqrt(6 * (miu20 + miu02 + deviation));
                component.MinorAxisLength = Math.Sqrt(6 * (miu20 + miu02 - deviation));
            }

            return component;
        }
    }
}
