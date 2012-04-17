using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Crop : Projection<IplImage, IplImage>
    {
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public CvRect RegionOfInterest { get; set; }

        public override IplImage Process(IplImage input)
        {
            if (RegionOfInterest.Width > 0 && RegionOfInterest.Height > 0)
            {
                using (input = new IplImage(input.Size, input.Depth, input.NumChannels, input.ImageData))
                {
                    var output = new IplImage(new CvSize(RegionOfInterest.Width, RegionOfInterest.Height), input.Depth, input.NumChannels);
                    input.ImageROI = RegionOfInterest;
                    Core.cvCopy(input, output);
                    input.ResetImageROI();
                    return output;
                }
            }

            return input;
        }
    }
}
