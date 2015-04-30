using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    class SampleBuffer
    {
        int offset;
        readonly long sampleIndex;
        readonly Mat samples;

        public SampleBuffer(Mat template, int count, long index)
        {
            sampleIndex = index;
            samples = new Mat(template.Rows, count, template.Depth, template.Channels);
        }

        public bool Refined { get; set; }

        public long SampleIndex
        {
            get { return sampleIndex; }
        }

        public Mat Samples
        {
            get { return samples; }
        }

        public bool Completed
        {
            get { return offset >= samples.Cols; }
        }

        public int Update(Mat source, int index)
        {
            var windowElements = Math.Min(source.Cols - index, samples.Cols - offset);
            if (windowElements > 0)
            {
                using (var dataSubRect = source.GetSubRect(new Rect(index, 0, windowElements, source.Rows)))
                using (var windowSubRect = samples.GetSubRect(new Rect(offset, 0, windowElements, samples.Rows)))
                {
                    CV.Copy(dataSubRect, windowSubRect);
                }

                offset += windowElements;
                return windowElements;
            }

            return 0;
        }
    }
}
