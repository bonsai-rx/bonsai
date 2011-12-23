using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class BinaryRegionAnalysisFilter : Filter<CvSeq, ConnectedComponentCollection>
    {
        ConnectedComponentCollection output;

        public override ConnectedComponentCollection Process(CvSeq input)
        {
            var contours = input;
            output.Clear();

            CvMoments moments;
            while (contours != null && !contours.IsInvalid)
            {
                ImgProc.cvMoments(contours, out moments, 0);

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
                    Contour = CvContour.FromCvSeq(contours)
                });

                contours = contours.HNext;
            }

            return output;
        }

        public override void Load(WorkflowContext context)
        {
            output = new ConnectedComponentCollection();
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            output.Clear();
            output = null;
            base.Unload(context);
        }
    }
}
