using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Reactive.Concurrency;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [Description("Produces a sequence with a single image loaded from the specified file.")]
    public class LoadImage : Source<IplImage>
    {
        public LoadImage()
        {
            Mode = LoadImageFlags.Unchanged;
        }

        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif")]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags Mode { get; set; }

        public override IObservable<IplImage> Generate()
        {
            return Observable.Defer(() =>
            {
                var fileName = FileName;
                if (string.IsNullOrEmpty(fileName))
                {
                    throw new InvalidOperationException("A valid image file path was not specified.");
                }

                var image = CV.LoadImage(FileName, Mode);
                if (image == null) throw new InvalidOperationException("Failed to load an image from the specified path.");
                return Observable.Return(image);
            });
        }
    }
}
