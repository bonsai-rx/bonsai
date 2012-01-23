using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
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

                var x = moments.m10 / moments.m00;
                var y = moments.m01 / moments.m00;
                var center = new CvPoint((int)x, (int)y);

                var miu20 = moments.m20 / moments.m00 - x * x;
                var miu02 = moments.m02 / moments.m00 - y * y;
                var miu11 = moments.m11 / moments.m00 - x * y;
                var angle = 0.5 * Math.Atan2(2 * miu11, (miu20 - miu02));

                output.Add(new ConnectedComponent
                {
                    Center = center,
                    Angle = angle,
                    Area = moments.m00,
                    Contour = CvContour.FromCvSeq(currentContour)
                });

                currentContour = currentContour.HNext;
            }

            return output;
        }
    }
}
