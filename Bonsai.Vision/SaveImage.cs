using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.IO;
using Bonsai.IO;
using System.Reactive.Linq;

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

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Do(input =>
            {
                var fileName = FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    PathHelper.EnsureDirectory(fileName);
                    fileName = PathHelper.AppendSuffix(fileName, Suffix);
                    HighGui.cvSaveImage(fileName, input);
                }
            });
        }
    }
}
