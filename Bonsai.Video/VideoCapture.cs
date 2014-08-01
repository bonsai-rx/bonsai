using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AForge.Video;
using System.Drawing;
using OpenCV.Net;
using System.Drawing.Imaging;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;

namespace Bonsai.Video
{
    public abstract class VideoCapture : Source<IplImage>
    {
        IVideoSource videoSource;
        IObservable<IplImage> source;

        public VideoCapture()
        {
            source = Observable.Using(
                () =>
                {
                    Load();
                    return Disposable.Create(Unload);
                },
                resource => Observable.FromEventPattern<NewFrameEventArgs>(
                    handler => videoSource.NewFrame += new NewFrameEventHandler(handler),
                    handler => videoSource.NewFrame -= new NewFrameEventHandler(handler))
                    .Select(evt =>
                    {
                        var bitmap = evt.EventArgs.Frame;
                        var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
                        try
                        {
                            var image = new IplImage(new OpenCV.Net.Size(bitmap.Width, bitmap.Height), IplDepth.U8, 3, bitmapData.Scan0);
                            var output = new IplImage(image.Size, image.Depth, image.Channels);
                            CV.Copy(image, output);
                            return output;
                        }
                        finally { bitmap.UnlockBits(bitmapData); }
                    }))
                    .PublishReconnectable()
                    .RefCount();
        }

        protected abstract IVideoSource CreateVideoSource();

        [Browsable(false)]
        public IVideoSource VideoSource
        {
            get { return videoSource; }
        }

        private void Load()
        {
            videoSource = CreateVideoSource();
            videoSource.Start();
        }

        private void Unload()
        {
            videoSource.SignalToStop();
            videoSource = null;
        }

        public override IObservable<IplImage> Generate()
        {
            return source;
        }
    }
}
