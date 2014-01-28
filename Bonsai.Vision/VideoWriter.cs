using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading.Tasks;
using Bonsai.IO;
using System.ComponentModel;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class VideoWriter : FileSink<IplImage, VideoWriterDisposable>
    {
        Size writerFrameSize;
        static readonly object SyncRoot = new object();

        public VideoWriter()
        {
            FourCC = "FMP4";
            FrameRate = 30;
        }

        public string FourCC { get; set; }

        public double FrameRate { get; set; }

        public Size FrameSize { get; set; }

        public SubPixelInterpolation ResizeInterpolation { get; set; }

        protected override VideoWriterDisposable CreateWriter(string fileName, IplImage input)
        {
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
