using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading.Tasks;
using Bonsai.IO;
using System.ComponentModel;
using System.Reactive.Disposables;
using System.IO;

namespace Bonsai.Vision
{
    [WorkflowElementIcon(typeof(ElementCategory), "ElementIcon.Video")]
    [Description("Writes the input image sequence into a compressed AVI file.")]
    public class VideoWriter : FileSink<IplImage, VideoWriterDisposable>
    {
        Size writerFrameSize;
        static readonly object SyncRoot = new object();

        public VideoWriter()
        {
            FourCC = "FMP4";
            FrameRate = 30;
        }

        [Description("The four-character code of the codec used to compress video frames.")]
        public string FourCC { get; set; }

        [Description("The playback frame rate of the image sequence.")]
        public double FrameRate { get; set; }

        [Description("The optional size of video frames.")]
        public Size FrameSize { get; set; }

        [Description("The optional interpolation method used to resize video frames.")]
        public SubPixelInterpolation ResizeInterpolation { get; set; }

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
