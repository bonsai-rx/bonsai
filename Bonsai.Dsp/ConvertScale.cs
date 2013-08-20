using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Converts the input matrix into the specified bit depth, with optional linear transformation.")]
    public class ConvertScale : Selector<CvMat, CvMat>
    {
        public ConvertScale()
        {
            Depth = CvMatDepth.CV_8U;
            Scale = 1;
        }

        [Description("The target bit depth of individual matrix elements.")]
        public CvMatDepth Depth { get; set; }

        [Description("The optional scale factor to apply to individual matrix elements.")]
        public double Scale { get; set; }

        [Description("The optional value to be added to individual matrix elements.")]
        public double Shift { get; set; }

        public override CvMat Process(CvMat input)
        {
            var output = new CvMat(input.Rows, input.Cols, Depth, input.NumChannels);
            Core.cvConvertScale(input, output, Scale, Shift);
            return output;
        }
    }
}
