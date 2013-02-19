using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class MaskImage : Transform<IplImage, IplImage>
    {
        CvArr mask;

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
            if (string.IsNullOrEmpty(FileName))
            {
                mask = CvArr.Null;
            }
            else mask = HighGui.cvLoadImage(FileName, LoadImageMode.Grayscale);
            return base.Load();
        }

        protected override void Unload()
        {
            if (mask != null && mask != CvArr.Null)
            {
                mask.Close();
            }
            base.Unload();
        }
    }
}
