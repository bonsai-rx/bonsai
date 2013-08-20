using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Crop : Selector<IplImage, IplImage>
    {
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", typeof(UITypeEditor))]
        public CvRect RegionOfInterest { get; set; }

        public override IplImage Process(IplImage input)
        {
            var rect = RegionOfInterest;
            if (rect.Width > 0 && rect.Height > 0)
            {
                return input.GetSubRect(rect);
            }

            return input;
        }
    }
}
