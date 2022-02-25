using System;
using OpenCV.Net;
using Bonsai.IO;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.IO;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that writes a sequence of images into a compressed AVI file.
    /// </summary>
    [WorkflowElementIcon(typeof(ElementCategory), "ElementIcon.Video")]
    [Description("Writes a sequence of images into a compressed AVI file.")]
    public class VideoWriter : FileSink<IplImage, VideoWriterDisposable>
    {
        Size writerFrameSize;
        static readonly object SyncRoot = new object();

        /// <summary>
        /// Gets or sets a value specifying the four-character code of the codec
        /// used to compress video frames.
        /// </summary>
        [Description("Specifies the four-character code of the codec used to compress video frames.")]
        public string FourCC { get; set; } = "FMP4";

        /// <summary>
        /// Gets or sets a value specifying the playback frame rate of the image sequence.
        /// </summary>
        [Description("Specifies the playback frame rate of the image sequence.")]
        public double FrameRate { get; set; } = 30;

        /// <summary>
        /// Gets or sets the optional size of video frames.
        /// </summary>
        [Description("The optional size of video frames.")]
        public Size FrameSize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the optional interpolation method used
        /// to resize video frames.
        /// </summary>
        [Description("Specifies the optional interpolation method used to resize video frames.")]
        public SubPixelInterpolation ResizeInterpolation { get; set; }

        /// <inheritdoc/>
        protected override VideoWriterDisposable CreateWriter(string fileName, IplImage input)
        {
            if (string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                throw new InvalidOperationException("The specified video file path must have a valid container extension (e.g. .avi).");
            }

            var frameSize = FrameSize.Width > 0 && FrameSize.Height > 0 ? FrameSize : input.Size;
            var fourCCText = FourCC;
            var fourCC = fourCCText.Length != 4 ? 0 : OpenCV.Net.VideoWriter.FourCC(fourCCText[0], fourCCText[1], fourCCText[2], fourCCText[3]);
            writerFrameSize = frameSize;
            lock (SyncRoot)
            {
                var writer = new OpenCV.Net.VideoWriter(fileName, fourCC, FrameRate, frameSize, input.Channels > 1);
                return new VideoWriterDisposable(writer, Disposable.Create(() =>
                {
                    lock (SyncRoot)
                    {
                        writer.Close();
                    }
                }));
            }
        }

        /// <summary>
        /// Writes an image into the compressed AVI file.
        /// </summary>
        /// <param name="writer">
        /// The <see cref="VideoWriterDisposable"/> object used to write data into the AVI file.
        /// </param>
        /// <param name="input">
        /// An <see cref="IplImage"/> object containing the video data to write into the file.
        /// </param>
        protected override void Write(VideoWriterDisposable writer, IplImage input)
        {
            if (input.Width != writerFrameSize.Width || input.Height != writerFrameSize.Height)
            {
                var resized = new IplImage(new Size(writerFrameSize.Width, writerFrameSize.Height), input.Depth, input.Channels);
                CV.Resize(input, resized, ResizeInterpolation);
                input = resized;
            }

            writer.Writer.WriteFrame(input);
        }
    }
}
