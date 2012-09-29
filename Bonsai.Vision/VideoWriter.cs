using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading.Tasks;
using Bonsai.IO;
using System.ComponentModel;

namespace Bonsai.Vision
{
    public class VideoWriter : Sink<IplImage>
    {
        Task writerTask;
        CvVideoWriter writer;
        CvSize writerFrameSize;

        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public string FileName { get; set; }

        public string FourCC { get; set; }

        public double FrameRate { get; set; }

        public CvSize FrameSize { get; set; }

        public bool IsColor { get; set; }

        public PathSuffix Suffix { get; set; }

        public SubPixelInterpolation ResizeInterpolation { get; set; }

        public override void Process(IplImage input)
        {
            if (writerTask == null) return;

            var runningWriter = writer;
            writerTask = writerTask.ContinueWith(task =>
            {
                if (runningWriter == null)
                {
                    PathHelper.EnsureDirectory(FileName);
                    var fileName = PathHelper.AppendSuffix(FileName, Suffix);
                    var frameSize = FrameSize.Width > 0 && FrameSize.Height > 0 ? FrameSize : input.Size;
                    var fourCCText = FourCC;
                    var fourCC = fourCCText.Length != 4 ? 0 : CvVideoWriter.FourCC(fourCCText[0], fourCCText[1], fourCCText[2], fourCCText[3]);
                    runningWriter = writer = new CvVideoWriter(FileName, fourCC, FrameRate, frameSize, IsColor);
                    writerFrameSize = frameSize;
                }

                if (input.Width != writerFrameSize.Width || input.Height != writerFrameSize.Height)
                {
                    var resized = new IplImage(new CvSize(writerFrameSize.Width, writerFrameSize.Height), input.Depth, input.NumChannels);
                    ImgProc.cvResize(input, resized, ResizeInterpolation);
                    input = resized;
                }

                runningWriter.WriteFrame(input);
            });
        }

        public override IDisposable Load()
        {
            if (!string.IsNullOrEmpty(FileName))
            {
                writerTask = new Task(() => { });
                writerTask.Start();
            }

            return base.Load();
        }

        protected override void Unload()
        {
            var closingWriter = writer;
            writerTask.ContinueWith(task => closingWriter.Close());

            writerTask = null;
            writer = null;
            base.Unload();
        }
    }
}
