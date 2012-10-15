using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class ImageSubtraction : Transform<IplImage, IplImage>
    {
        CvArr image;

        public ImageSubtraction()
        {
            Format = LoadImageMode.Grayscale;
        }

        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public LoadImageMode Format { get; set; }

        public override IplImage Process(IplImage input)
        {
            var output = new IplImage(input.Size, input.Depth, input.NumChannels);
            Core.cvAbsDiff(input, image, output);
            return output;
        }

        public override IDisposable Load()
        {
            image = HighGui.cvLoadImage(FileName, Format);
            return base.Load();
        }

        protected override void Unload()
        {
            image.Close();
            image = null;
            base.Unload();
        }
    }
}
