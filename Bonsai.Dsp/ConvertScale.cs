using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Converts the input matrix into the specified bit depth, with optional linear transformation.")]
    public class ConvertScale : Transform<Mat, Mat>
    {
        public ConvertScale()
        {
            Depth = Depth.U8;
            Scale = 1;
        }

        [Description("The target bit depth of individual matrix elements.")]
        public Depth Depth { get; set; }

        [Description("The optional scale factor to apply to individual matrix elements.")]
        public double Scale { get; set; }

        [Description("The optional value to be added to individual matrix elements.")]
        public double Shift { get; set; }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var output = new Mat(input.Rows, input.Cols, Depth, input.Channels);
                CV.ConvertScale(input, output, Scale, Shift);
                return output;
            });
        }
    }
}
