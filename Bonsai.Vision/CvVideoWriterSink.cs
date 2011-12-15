using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Vision
{
    public class CvVideoWriterSink : Sink<IplImage>
    {
        CvVideoWriter writer;

        public string FileName { get; set; }

        public int FourCC { get; set; }

        public int FrameRate { get; set; }

        public override void Process(IplImage input)
        {
            writer.WriteFrame(input);
        }

        public override void Load(WorkflowContext context)
        {
            var size = (CvSize)context.GetService(typeof(CvSize));
            writer = new CvVideoWriter(FileName, FourCC, FrameRate, size);
        }

        public override void Unload()
        {
            writer.Close();
        }
    }
}
