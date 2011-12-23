using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class FindContoursFilter : Filter<IplImage, CvSeq>
    {
        IplImage temp;
        CvMemStorage storage;

        public ContourRetrieval Mode { get; set; }

        public ContourApproximation Method { get; set; }

        public CvPoint Offset { get; set; }

        public double MinArea { get; set; }

        public override CvSeq Process(IplImage input)
        {
            CvSeq currentContour;
            Core.cvCopy(input, temp);

            storage.Clear();
            var scanner = ImgProc.cvStartFindContours(temp, storage, CvContour.HeaderSize, Mode, Method, Offset);
            while (!(currentContour = scanner.FindNextContour()).IsInvalid)
            {
                if (MinArea > 0 && ImgProc.cvContourArea(currentContour, CvSlice.WholeSeq, 0) < MinArea)
                {
                    scanner.SubstituteContour(CvSeq.Null);
                }
            }

            return scanner.EndFindContours();
        }

        public override void Load(WorkflowContext context)
        {
            var size = (CvSize)context.GetService(typeof(CvSize));
            temp = new IplImage(size, 8, 1);
            storage = new CvMemStorage();
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            storage.Close();
            base.Unload(context);
        }
    }
}
