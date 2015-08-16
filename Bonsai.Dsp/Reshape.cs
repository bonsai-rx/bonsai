using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Changes shape of input array without copying data.")]
    public class Reshape
    {
        [Description("The new number of channels. Zero means that the number of channels remains unchanged.")]
        public int Channels { get; set; }

        [Description("The new number of rows. Zero means that the number of rows remains unchanged.")]
        public int Rows { get; set; }

        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input => input.Reshape(Channels, Rows));
        }

        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input => input.Reshape(Channels, Rows).GetImage());
        }
    }
}
