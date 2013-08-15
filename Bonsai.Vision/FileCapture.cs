using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
using System.Diagnostics;
using System.Threading;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    [Description("Produces a video sequence of images from the specified movie file.")]
    [Editor("Bonsai.Vision.Design.FileCaptureEditor, Bonsai.Vision.Design", typeof(ComponentEditor))]
    public class FileCapture : Source<IplImage>
    {
        int? targetFrame;
        CvCapture capture;
        double captureFps;
        IplImage image;
        IObservable<IplImage> source;

        public FileCapture()
        {
            Playing = true;

            var stopwatch = new Stopwatch();
            source = Observable.Using(
                () =>
                {
                    var fileName = FileName;
                    if (string.IsNullOrEmpty(fileName))
                    {
                        throw new InvalidOperationException("A valid video file path was not specified.");
                    }

                    image = null;
                    capture = CvCapture.CreateFileCapture(fileName);
                    if (capture.IsInvalid) throw new InvalidOperationException("Failed to open the video at the specified path.");
                    captureFps = capture.GetProperty(CaptureProperty.FPS);
                    return capture;
                },
                capture => ObservableCombinators.GenerateWithThread<IplImage>(observer =>
                {
                    stopwatch.Restart();
                    if (targetFrame.HasValue)
                    {
                        var target = targetFrame.Value;
                        var currentFrame = (int)capture.GetProperty(CaptureProperty.POS_FRAMES) - 1;
                        if (target != currentFrame)
                        {
                            capture.SetProperty(CaptureProperty.POS_FRAMES, target);
                            if (target < currentFrame) // seek backward
                            {
                                currentFrame = (int)capture.GetProperty(CaptureProperty.POS_FRAMES);
                                image = capture.QueryFrame();

                                int skip = 1;
                                while (target < currentFrame)
                                {
                                    // try to seek back to the nearest key frame in multiples of two
                                    capture.SetProperty(CaptureProperty.POS_FRAMES, target - skip);
                                    currentFrame = (int)capture.GetProperty(CaptureProperty.POS_FRAMES);
                                    skip *= 2;
                                }
                            }

                            // continue seeking frame-by-frame until target is reached
                            while (target > currentFrame)
                            {
                                currentFrame = (int)capture.GetProperty(CaptureProperty.POS_FRAMES);
                                var nextFrame = capture.QueryFrame();

                                // if next frame is null we tried to seek past the end of the file
                                if (nextFrame == null) break;
                                image = nextFrame;
                            }
                        }

                        targetFrame = null;
                    }
                    else if (Playing || image == null)
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
                }))
                .PublishReconnectable()
                .RefCount();
        }

        [Browsable(false)]
        public CvCapture Capture
        {
            get { return capture; }
        }

        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [Description("The name of the movie file.")]
        public string FileName { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The rate at which to read images from the file. A value of 0 means the native video frame rate will be used.")]
        public double PlaybackRate { get; set; }

        [Description("Indicates whether the video sequence should loop when the end of the file is reached.")]
        public bool Loop { get; set; }

        [Description("Allows the video sequence to be paused or resumed.")]
        public bool Playing { get; set; }

        public void Seek(int frameNumber)
        {
            targetFrame = frameNumber;
        }

        public override IObservable<IplImage> Generate()
        {
            return source;
        }
    }
}
