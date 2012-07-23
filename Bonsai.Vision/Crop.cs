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
            var rect = RegionOfInterest;
            if (rect.Width > 0 && rect.Height > 0)
            {
                try
                {
                    var output = new IplImage(new CvSize(rect.Width, rect.Height), input.Depth, input.NumChannels);
                    using (var header = new IplImage(input.Size, input.Depth, input.NumChannels, input.ImageData))
                    {
                        header.ImageROI = rect;
                        Core.cvCopy(header, output);
                    }

                    return output;
                }
                finally { GC.KeepAlive(input); }
            }

            return input;
        }
    }
}
