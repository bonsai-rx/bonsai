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
                using (var image = new IplImage(input.Size, input.Depth, input.NumChannels, input.ImageData))
                {
                    var output = new IplImage(new CvSize(RegionOfInterest.Width, RegionOfInterest.Height), input.Depth, input.NumChannels);
                    image.ImageROI = RegionOfInterest;
                    Core.cvCopy(image, output);
                    GC.KeepAlive(input);
                    return output;
                }
            }

            return input;
        }
    }
}
