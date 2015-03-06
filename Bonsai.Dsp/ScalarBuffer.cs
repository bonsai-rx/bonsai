using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Description("Produces a sequence with a single buffer where all elements are set to the same scalar value.")]
    public class ScalarBuffer : Source<Mat>
    {
        public ScalarBuffer()
        {
            Depth = OpenCV.Net.Depth.F32;
            Channels = 1;
        }

        [Description("The size of the output buffer.")]
        public Size Size { get; set; }

        [Description("The target bit depth of individual buffer elements.")]
        public Depth Depth { get; set; }

        [Description("The number of channels in the output buffer.")]
        public int Channels { get; set; }

        [Description("The scalar value to which all element in the output buffer will be set to.")]
        public Scalar Value { get; set; }

        public override IObservable<Mat> Generate()
        {
            return Observable.Defer(() =>
            {
                var buffer = new Mat(Size, Depth, Channels);
                buffer.Set(Value);
                return Observable.Return(buffer);
            });
        }
    }
}
