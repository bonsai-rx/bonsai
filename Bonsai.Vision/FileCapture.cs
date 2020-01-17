using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [DefaultProperty("FileName")]
    [WorkflowElementIcon(typeof(ElementCategory), "ElementIcon.Video")]
    [Description("Produces a sequence of images from the specified movie file.")]
    [Editor("Bonsai.Vision.Design.FileCaptureEditor, Bonsai.Vision.Design", typeof(ComponentEditor))]
    public class FileCapture : Source<IplImage>
    {
        int? targetPosition;
        Capture captureCache;
        double captureFps;

        public FileCapture()
        {
            Playing = true;
            PositionUnits = CapturePosition.Frames;
        }

        [Browsable(false)]
        public Capture Capture
        {
            get { return captureCache; }
        }

        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the movie file.")]
        public string FileName { get; set; }

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The rate at which to read images from the file. A value of 0 means the native video frame rate will be used.")]
        public double PlaybackRate { get; set; }

        [Description("The position at which to start playback of the file.")]
        public double StartPosition { get; set; }

        [Description("The units in which the start position is specified.")]
        public CapturePosition PositionUnits { get; set; }

        [Description("Indicates whether the video sequence should loop when the end of the file is reached.")]
        public bool Loop { get; set; }

        [Description("Allows the video sequence to be paused or resumed.")]
        public bool Playing { get; set; }

        public void Seek(int frameNumber)
        {
            targetPosition = frameNumber;
        }

        private int GetFramePosition(double position, CapturePosition units)
        {
            switch (units)
            {
                case CapturePosition.Milliseconds:
                    return (int)(position * captureFps / 1000.0);
                case CapturePosition.AviRatio:
                    var frameCount = captureCache.GetProperty(CaptureProperty.FrameCount);
                    return (int)(position * frameCount);
                case CapturePosition.Frames:
                default: return (int)position;
            }
        }

        IEnumerable<IplImage> CreateCapture()
        {
            var fileName = FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("A valid video file path was not specified.");
            }

            var image = default(IplImage);
            var capture = Capture.CreateFileCapture(fileName);
            if (capture == null) throw new InvalidOperationException("Failed to open the video at the specified path.");
            captureCache = capture;

            try
            {
                captureFps = capture.GetProperty(CaptureProperty.Fps);
                var position = StartPosition;
                if (position > 0)
                {
                    targetPosition = GetFramePosition(position, PositionUnits);
                }

                while (true)
                {
                    if (targetPosition.HasValue)
                    {
                        var target = targetPosition.Value;
                        var currentPosition = (int)capture.GetProperty(CaptureProperty.PosFrames) - 1;
                        if (target != currentPosition)
                        {
                            IplImage targetFrame = null;
                            capture.SetProperty(CaptureProperty.PosFrames, target);
                            if (target < currentPosition) // seek backward
                            {
                                currentPosition = (int)capture.GetProperty(CaptureProperty.PosFrames);
                                targetFrame = capture.QueryFrame();

                                int skip = 1;
                                while (target < currentPosition)
                                {
                                    // try to seek back to the nearest key frame in multiples of two
                                    capture.SetProperty(CaptureProperty.PosFrames, target - skip);
                                    currentPosition = (int)capture.GetProperty(CaptureProperty.PosFrames);
                                    skip *= 2;
                                }
                            }

                            // continue seeking frame-by-frame until target is reached
                            while (target > currentPosition)
                            {
                                currentPosition = (int)capture.GetProperty(CaptureProperty.PosFrames);
                                var nextFrame = capture.QueryFrame();

                                // if next frame is null we tried to seek past the end of the file
                                if (nextFrame == null) break;
                                targetFrame = nextFrame;
                            }

                            // unable to switch to a valid frame; possibly truncated file
                            if (targetFrame == null)
                            {
                                targetPosition = null;
                                image = null;
                                continue;
                            }

                            // successfully switched frame; clone it for cache
                            image = targetFrame.Clone();
                        }

                        targetPosition = null;
                    }
                    else if (Playing || image == null)
                    {
                        var currentFrame = capture.QueryFrame();
                        if (currentFrame == null)
                        {
                            if (Loop)
                            {
                                capture.SetProperty(CaptureProperty.PosFrames, 0);
                                currentFrame = capture.QueryFrame();
                            }
                            else yield break;
                        }

                        // successfully switched frame; clone it for cache
                        image = currentFrame.Clone();
                    }

                    yield return image;
                }
            }
            finally
            {
                capture.Dispose();
                captureCache = null;
            }
        }

        public override IObservable<IplImage> Generate()
        {
            return Observable.Create<IplImage>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using (var reader = CreateCapture().GetEnumerator())
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        var stopwatch = new Stopwatch();
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            stopwatch.Restart();
                            if (!reader.MoveNext()) break;
                            observer.OnNext(reader.Current);

                            var playbackRate = PlaybackRate;
                            var targetFps = playbackRate > 0 ? playbackRate : captureFps;
                            var dueTime = Math.Max(0, (1000.0 / targetFps) - stopwatch.Elapsed.TotalMilliseconds);
                            if (dueTime > 0)
                            {
                                sampleSignal.WaitOne(TimeSpan.FromMilliseconds(dueTime));
                            }
                        }

                        observer.OnCompleted();
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Zip(CreateCapture(), (x, image) => image);
        }
    }

    public enum CapturePosition
    {
        Milliseconds,
        Frames,
        AviRatio
    }
}
