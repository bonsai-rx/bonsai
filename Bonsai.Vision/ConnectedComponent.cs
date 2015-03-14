﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        public Contour Contour { get; set; }

        public static ConnectedComponent FromContour(Seq currentContour)
        {
            var moments = new Moments(currentContour);
            var component = new ConnectedComponent();
            component.Area = moments.M00;
            component.Contour = Contour.FromSeq(currentContour);

            // Cemtral moments can only be computed for components with non-zero area
            if (moments.M00 > 0)
            {
                // Compute centroid components
                var x = moments.M10 / moments.M00;
                var y = moments.M01 / moments.M00;
                component.Centroid = new Point2f((float)x, (float)y);

                // Compute second-order central moments
                var miu20 = moments.M20 / moments.M00 - x * x;
                var miu02 = moments.M02 / moments.M00 - y * y;
                var miu11 = moments.M11 / moments.M00 - x * y;

                // Compute orientation and major/minor axis length
                var b = 2 * miu11;
                component.Orientation = 0.5 * Math.Atan2(b, miu20 - miu02);
                var deviation = Math.Sqrt(b * b + Math.Pow(miu20 - miu02, 2));
                component.MajorAxisLength = Math.Sqrt(6 * (miu20 + miu02 + deviation));
                component.MinorAxisLength = Math.Sqrt(6 * (miu20 + miu02 - deviation));
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
