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
    [Description("Performs a forward or inverse Discrete Fourier transform of a 1D or 2D array.")]
    public class DiscreteFourierTransform : ArrayTransform
    {
        [Description("Specifies the operation of the DFT.")]
        public DiscreteTransformFlags OperationFlags { get; set; }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateSizeFactory;
            var inputFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return source.Select(input =>
            {
                var output = outputFactory(input, Depth.F32, 2);
                if (input.ElementType != output.ElementType)
                {
                    var temp = inputFactory(input, Depth.F32);
                    CV.Convert(input, temp);
                    input = temp;
                }

                CV.DFT(input, output, OperationFlags, 0);
                return output;
            });
        }
    }
}
