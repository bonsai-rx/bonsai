using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class SaveImage : Sink<IplImage>
    {
        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public override void Process(IplImage input)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                HighGui.cvSaveImage(FileName, input);
            }
        }
    }
}
