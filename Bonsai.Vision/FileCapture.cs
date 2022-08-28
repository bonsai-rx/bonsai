using System;
using System.Collections.Generic;
using OpenCV.Net;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that generates a sequence of images from the
    /// specified movie file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [WorkflowElementIcon(typeof(ElementCategory), "ElementIcon.Video")]
    [Description("Generates a sequence of images from the specified movie file.")]
    public class FileCapture : Source<IplImage>
    {
        int? targetPosition;
        Capture captureCache;
        double captureFps;

        /// <summary>
        /// Gets the last active video capture stream. This property is reserved
        /// to be used only by the file capture visualizer.
        /// </summary>
        [Browsable(false)]
        public Capture Capture
        {
            get { return captureCache; }
        }

        /// <summary>
        /// Gets or sets the name of the movie file.
        /// </summary>
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the movie file.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the rate at which to read images from the file. A value
        /// of zero means the recorded video frame rate will be used.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The rate at which to read images from the file. A value of zero means the recorded video frame rate will be used.")]
        public double PlaybackRate { get; set; }

        /// <summary>
        /// Gets or sets the position at which to start playback of the file.
        /// </summary>
        [Description("The position at which to start playback of the file.")]
        public double StartPosition { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the units of the start position.
        /// </summary>
        [Description("Specifies the units of the start position.")]
        public CapturePosition PositionUnits { get; set; } = CapturePosition.Frames;

        /// <summary>
        /// Gets or sets a value indicating whether the video sequence should
        /// loop when the end of the file is reached.
        /// </summary>
        [Description("Indicates whether the video sequence should loop when the end of the file is reached.")]
        public bool Loop { get; set; }

        /// <summary>
        /// Gets or sets a value specifying whether the video sequence is playing.
        /// If the video is paused, the current frame will be repeated at the specified
        /// playback rate.
        /// </summary>
        [Description("Specifies whether the video sequence is playing. If the video is paused, the current frame will be repeated at the specified playback rate.")]
        public bool Playing { get; set; } = true;

        /// <summary>
        /// Moves the current video player to the specified frame. This method
        /// is reserved to be called by the file capture visualizer.
        /// </summary>
        /// <param name="frameNumber">
        /// The zero-based index of the frame the player should move to.
        /// </param>
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
            captureCache = capture ?? throw new InvalidOperationException("Failed to open the video at the specified path.");

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

        /// <summary>
        /// Generates an observable sequence of images from the specified movie file.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing each of
        /// the frames in the specified movie file.
        /// </returns>
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

        /// <summary>
        /// Generates a sequence of images from the specified movie file, where each
        /// new image is emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for reading new images from the
        /// movie file.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing each of
        /// the frames in the specified movie file.
        /// </returns>
        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Zip(CreateCapture(), (x, image) => image);
        }
    }

    /// <summary>
    /// Specifies the units of the file capture start position.
    /// </summary>
    public enum CapturePosition
    {
        /// <summary>
        /// A value in milliseconds representing time from the start of the video.
        /// </summary>
        Milliseconds,

        /// <summary>
        /// The zero-based index of a video frame.
        /// </summary>
        Frames,

        /// <summary>
        /// A relative position in the file, where zero is the start of the
        /// video and one is the end of the video.
        /// </summary>
        AviRatio
    }
}
