using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    public class ImageSubtraction : Transform<IplImage, IplImage>
    {
        public ImageSubtraction()
        {
            Format = LoadImageMode.Grayscale;
        }

        [FileNameFilter("PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public LoadImageMode Format { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Using(
                () => HighGui.cvLoadImage(FileName, Format),
                image => source.Select(input =>
                {
                    var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                    Core.cvAbsDiff(input, image, output);
                    return output;
                }));
        }
    }
}
