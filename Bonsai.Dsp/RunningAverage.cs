using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Dsp
{
    public abstract class RunningAverage<TArray> : Transform<TArray, TArray> where TArray : CvArr
    {
        TArray accumulator;

        [Range(0, 1)]
        [Precision(2, .01)]
        [Editor(DesignTypes.TrackbarEditor, typeof(UITypeEditor))]
        public double Alpha { get; set; }

        protected abstract TArray CreateArray(TArray source, CvMatDepth depth);

        public override TArray Process(TArray input)
        {
            if (accumulator == null)
            {
                accumulator = CreateArray(input, CvMatDepth.CV_32F);
                Core.cvConvertScale(input, accumulator, 1, 0);
                return input;
            }
            else
            {
                var output = CreateArray(input, 0);
                ImgProc.cvRunningAvg(input, accumulator, Alpha, CvArr.Null);
                Core.cvConvertScale(accumulator, output, 1, 0);
                return output;
            }
        }

        protected override void Unload()
        {
            if (accumulator != null)
            {
                accumulator.Close();
                accumulator = null;
            }
            base.Unload();
        }
    }

    public class RunningAverage : RunningAverage<CvMat>
    {
        protected override CvMat CreateArray(CvMat source, CvMatDepth depth)
        {
            depth = depth > 0 ? depth : source.Depth;
            return new CvMat(source.Rows, source.Cols, depth, source.NumChannels);
        }
    }
}
