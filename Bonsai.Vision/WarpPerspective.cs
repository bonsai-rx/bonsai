using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class WarpPerspective : Filter<IplImage, IplImage>
    {
        IplImage output;
        CvMat mapMatrix;
        CvPoint2D32f[] currentSource;
        CvPoint2D32f[] currentDestination;

        [Editor("Bonsai.Vision.Design.IplImageInputQuadrangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public CvPoint2D32f[] Source { get; set; }

        [Editor("Bonsai.Vision.Design.IplImageOutputQuadrangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public CvPoint2D32f[] Destination { get; set; }

        static CvPoint2D32f[] InitializeQuadrangle(IplImage image)
        {
            return new[]
            {
                new CvPoint2D32f(0, 0),
                new CvPoint2D32f(0, image.Height),
                new CvPoint2D32f(image.Width, image.Height),
                new CvPoint2D32f(image.Width, 0)
            };
        }

        public override IplImage Process(IplImage input)
        {
            if (output == null || !output.FormatEquals(input))
            {
                output = new IplImage(input.Size, input.Depth, input.NumChannels);
                Source = Source ?? InitializeQuadrangle(output);
                Destination = Destination ?? InitializeQuadrangle(output);
            }

            if (Source != currentSource || Destination != currentDestination)
            {
                currentSource = Source;
                currentDestination = Destination;
                ImgProc.cvGetPerspectiveTransform(currentSource, currentDestination, mapMatrix);
            }

            ImgProc.cvWarpPerspective(input, output, mapMatrix, WarpFlags.Linear | WarpFlags.FillOutliers, CvScalar.All(0));
            return output;
        }

        public override void Load(WorkflowContext context)
        {
            mapMatrix = new CvMat(3, 3, CvMatDepth.CV_32F, 1);
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            mapMatrix.Close();
            if (output != null)
            {
                output.Close();
                output = null;
            }
            base.Unload(context);
        }
    }
}
