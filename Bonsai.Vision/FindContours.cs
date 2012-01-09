using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class FindContours : Projection<IplImage, Contours>
    {
        IplImage temp;

        public FindContours()
        {
            Method = ContourApproximation.CHAIN_APPROX_NONE;
        }

        public ContourRetrieval Mode { get; set; }

        public ContourApproximation Method { get; set; }

        public CvPoint Offset { get; set; }

        public double MinArea { get; set; }

        public override Contours Process(IplImage input)
        {
            CvSeq currentContour;
            temp = IplImageHelper.EnsureImageFormat(temp, input.Size, 8, 1);
            Core.cvCopy(input, temp);

            var storage = new CvMemStorage();
            var scanner = ImgProc.cvStartFindContours(temp, storage, CvContour.HeaderSize, Mode, Method, Offset);
            while (!(currentContour = scanner.FindNextContour()).IsInvalid)
            {
                if (MinArea > 0 && ImgProc.cvContourArea(currentContour, CvSlice.WholeSeq, 0) < MinArea)
                {
                    scanner.SubstituteContour(CvSeq.Null);
                }
            }

            return new Contours(scanner.EndFindContours(), input.Size);
        }

        protected override void Unload()
        {
            if (temp != null)
            {
                temp.Close();
                temp = null;
            }
            base.Unload();
        }
    }
}
