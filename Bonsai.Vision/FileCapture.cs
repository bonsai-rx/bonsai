using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using System.Threading;

namespace Bonsai.Vision
{
    public class FileCapture : Source<IplImage>
    {
        CvCapture capture;
        Stopwatch stopwatch;
        double captureFps;
        IplImage image;

        public FileCapture()
        {
            stopwatch = new Stopwatch();
            Playing = true;
        }

        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public double PlaybackRate { get; set; }

        public bool Loop { get; set; }

        public bool Playing { get; set; }

        public override IDisposable Load()
        {
            capture = CvCapture.CreateFileCapture(FileName);
            captureFps = capture.GetProperty(CaptureProperty.FPS);
            return base.Load();
        }

        protected override void Unload()
        {
            capture.Close();
            image = null;
            base.Unload();
        }

        protected override IObservable<IplImage> Generate()
        {
            return ObservableCombinators.GenerateWithThread<IplImage>(observer =>
            {
                stopwatch.Restart();
                if (Playing || image == null)
                {
                    image = capture.QueryFrame();
                    if (image == null)
                    {
                        if (Loop)
                        {
                            capture.SetProperty(CaptureProperty.POS_FRAMES, 0);
                            image = capture.QueryFrame();
                        }
                        else
                        {
                            observer.OnCompleted();
                            return;
                        }
                    }
                }

                var targetFps = PlaybackRate > 0 ? PlaybackRate : captureFps;
                var dueTime = Math.Max(0, (1000.0 / targetFps) - stopwatch.Elapsed.TotalMilliseconds);
                if (dueTime > 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(dueTime));
                }

                observer.OnNext(image.Clone());
            });
        }
    }
}
