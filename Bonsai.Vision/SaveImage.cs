using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using Bonsai.IO;

namespace Bonsai.Vision
{
    [Description("Writes the input image into the specified file.")]
    public class SaveImage : Sink<IplImage>
    {
        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        public PathSuffix Suffix { get; set; }

        public override void Process(IplImage input)
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                PathHelper.EnsureDirectory(FileName);
                var fileName = PathHelper.AppendSuffix(FileName, Suffix);
                HighGui.cvSaveImage(fileName, input);
            }
        }
    }
}
