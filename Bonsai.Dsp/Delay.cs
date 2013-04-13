using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;

namespace Bonsai.Dsp
{
    public class Delay : Transform<CvMat, CvMat>
    {
        int bufferIndex;
        Queue<CvMat> buffer;

        public int Count { get; set; }

        public override CvMat Process(CvMat input)
        {
            if (buffer == null)
            {
                buffer = new Queue<CvMat>();
                var delayBuffer = new CvMat(input.Rows, Count, input.Depth, input.NumChannels);
                buffer.Enqueue(delayBuffer);
                delayBuffer.SetZero();
            }

            buffer.Enqueue(input);
            var output = new CvMat(input.Rows, input.Cols, input.Depth, input.NumChannels);
            var outputRemainder = output.Cols;
            var outputIndex = 0;

            while (outputRemainder > 0)
            {
                var currentBuffer = buffer.Peek();
                var sampleCount = Math.Min(currentBuffer.Cols - bufferIndex, outputRemainder);
                using (var bufferRoi = currentBuffer.GetSubRect(new CvRect(bufferIndex, 0, sampleCount, currentBuffer.Rows)))
                using (var outputRoi = output.GetSubRect(new CvRect(outputIndex, 0, sampleCount, currentBuffer.Rows)))
                {
                    Core.cvCopy(bufferRoi, outputRoi);
                    outputRemainder -= sampleCount;
                    outputIndex += sampleCount;
                    bufferIndex = (bufferIndex + sampleCount) % currentBuffer.Cols;
                    if (bufferIndex == 0) buffer.Dequeue();
                }
            }

            return output;
        }

        protected override void Unload()
        {
            buffer = null;
            bufferIndex = 0;
            base.Unload();
        }
    }
}
