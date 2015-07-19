using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Editor("Bonsai.Vision.Design.PolarTransformEditor, Bonsai.Vision.Design", typeof(ComponentEditor))]
    public abstract class PolarTransform : Transform<IplImage, IplImage>
    {
        public PolarTransform()
        {
            Flags = WarpFlags.Linear | WarpFlags.FillOutliers;
        }

        [Description("The transformation center where the output precision is maximal.")]
        public Point2f Center { get; set; }

        [Description("The magnitude scale parameter for polar transformation.")]
        public double Magnitude { get; set; }

        [Description("Specifies interpolation and operation flags for the image warp.")]
        public WarpFlags Flags { get; set; }
    }
}
