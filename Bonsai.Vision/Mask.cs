using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class Mask : Projection<IplImage, IplImage>
    {
        IplImage mask;

        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            output.SetZero();
            Core.cvCopy(input, output, mask);
            return output;
        }

        public override IDisposable Load()
        {
            mask = HighGui.cvLoadImage(FileName, LoadImageMode.Grayscale);
            return base.Load();
        }

        protected override void Unload()
        {
            mask.Close();
            base.Unload();
        }
    }
}
