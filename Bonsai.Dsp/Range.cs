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
    [Description("Produces a sequence of buffers filled with a specified range of numbers.")]
    public class Range : Source<Mat>
    {
        public Range()
        {
            Depth = OpenCV.Net.Depth.F32;
        }

        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; }

        [TypeConverter(typeof(DepthConverter))]
        [Description("The target bit depth of individual buffer elements.")]
        public Depth Depth { get; set; }

        [Description("The inclusive lower bound of the range.")]
        public double Start { get; set; }

        [Description("The exclusive upper bound of the range.")]
        public double End { get; set; }

        Mat CreateBuffer()
        {
            var buffer = new Mat(1, BufferLength, Depth, 1);
            CV.Range(buffer, Start, End);
            return buffer;
        }

        public override IObservable<Mat> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateBuffer()));
        }

        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateBuffer());
        }

        class DepthConverter : EnumConverter
        {
            public DepthConverter(Type type)
                : base(type)
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { Depth.S32, Depth.F32 });
            }
        }
    }
}
