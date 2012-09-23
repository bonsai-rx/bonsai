using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Computes image moments of connected component contours to extract binary region properties.")]
    public class BinaryRegionAnalysis : Projection<Contours, ConnectedComponentCollection>
    {
        public override ConnectedComponentCollection Process(Contours input)
        {
            var currentContour = input.FirstContour;
            var output = new ConnectedComponentCollection(input.ImageSize);

            CvMoments moments;
            while (currentContour != null && !currentContour.IsInvalid)
            {
                ImgProc.cvMoments(currentContour, out moments, 0);

                // Moments can only be computed for components with non-zero area
                if (moments.m00 > 0)
                {
                    // Compute centroid components
                    var x = moments.m10 / moments.m00;
                    var y = moments.m01 / moments.m00;
                    var centroid = new CvPoint2D32f((float)x, (float)y);

                    // Compute second-order central moments
                    var miu20 = moments.m20 / moments.m00 - x * x;
                    var miu02 = moments.m02 / moments.m00 - y * y;
                    var miu11 = moments.m11 / moments.m00 - x * y;

                    // Compute orientation and major/minor axis length
                    var b = 2 * miu11;
                    var orientation = 0.5 * Math.Atan2(b, miu20 - miu02);
                    var deviation = Math.Sqrt(b * b + Math.Pow(miu20 - miu02, 2));
                    var majorAxisLength = Math.Sqrt(6 * (miu20 + miu02 + deviation));
                    var minorAxisLength = Math.Sqrt(6 * (miu20 + miu02 - deviation));

                    output.Add(new ConnectedComponent
                    {
                        Area = moments.m00,
                        Centroid = centroid,
                        Orientation = orientation,
                        MajorAxisLength = majorAxisLength,
                        MinorAxisLength = minorAxisLength,
                        Contour = CvContour.FromCvSeq(currentContour)
                    });
                }

                currentContour = currentContour.HNext;
            }

            return output;
        }
    }
}
