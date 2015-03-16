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
                resource =>
                {
                    var newFrame = Observable.FromEventPattern<NewFrameEventHandler, NewFrameEventArgs>(
                        handler => videoSource.NewFrame += handler,
                        handler => videoSource.NewFrame -= handler)
                        .Select(evt => ProcessFrame(evt.EventArgs.Frame));
                    var errors = Observable.FromEventPattern<VideoSourceErrorEventHandler, VideoSourceErrorEventArgs>(
                        handler => videoSource.VideoSourceError += handler,
                        handler => videoSource.VideoSourceError -= handler)
                        .SelectMany(evt => Observable.Throw<IplImage>(new VideoException(evt.EventArgs.Description)));
                    var completed = Observable.Create<IplImage>(observer =>
                    {
                        PlayingFinishedEventHandler handler = delegate { observer.OnCompleted(); };
                        videoSource.PlayingFinished += handler;
                        return Disposable.Create(() => videoSource.PlayingFinished -= handler);
                    });
                    return newFrame.Merge(errors).TakeUntil(completed);
                })
                .PublishReconnectable()
                .RefCount();
        }

        protected abstract IVideoSource CreateVideoSource();

        protected virtual IplImage ProcessFrame(Bitmap bitmap)
        {
            var bitmapData = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);
            try
            {
                var image = new IplImage(new OpenCV.Net.Size(bitmap.Width, bitmap.Height), IplDepth.U8, 3, bitmapData.Scan0);
                var output = new IplImage(image.Size, image.Depth, image.Channels);
                CV.Copy(image, output);
                return output;
            }
            finally { bitmap.UnlockBits(bitmapData); }
        }

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
