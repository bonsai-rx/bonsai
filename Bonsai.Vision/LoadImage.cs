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
    [Description("Produces a sequence with a single image loaded from the specified file.")]
    public class LoadImage : Source<IplImage>
    {
        IplImage image;

        public LoadImage()
        {
            Mode = LoadImageMode.Unchanged;
        }

        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif|PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif")]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageMode Mode { get; set; }

        public override IDisposable Load()
        {
            image = HighGui.cvLoadImage(FileName, Mode);
            return base.Load();
        }

        protected override void Unload()
        {
            image.Dispose();
            image = null;
            base.Unload();
        }

        protected override IObservable<IplImage> Generate()
        {
            return Observable.Return(image);
        }
    }
}
