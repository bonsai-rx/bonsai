using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.IO;
using Bonsai.IO;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Writes the input image into the specified file.")]
    public class SaveImage : Sink<IplImage>
    {
        [FileNameFilter("PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the output image file.")]
        public string FileName { get; set; }

        [Description("The optional suffix used to generate file names.")]
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
                    CV.SaveImage(fileName, input);
                }
            });
        }
    }
}
