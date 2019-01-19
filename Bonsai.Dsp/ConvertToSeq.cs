using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts a fixed size array type into a sequence of elements.")]
    public class ConvertToSeq
    {
        [TypeConverter(typeof(FlagsConverter))]
        [Description("The operation flags for the element sequence.")]
        public SequenceFlags Flags { get; set; }

        Seq FromArray<TElement>(TElement[] input, Depth depth, int channels) where TElement : struct
        {
            var storage = new MemStorage();
            var output = new Seq(depth, channels, SequenceKind.Curve, Flags, storage);
            if (input.Length > 0)
            {
                output.Push(input);
            }
            return output;
        }

        public IObservable<Seq> Process(IObservable<byte[]> source)
        {
            return source.Select(input => FromArray(input, Depth.S8, 1));
        }

        public IObservable<Seq> Process(IObservable<short[]> source)
        {
            return source.Select(input => FromArray(input, Depth.S16, 1));
        }

        public IObservable<Seq> Process(IObservable<ushort[]> source)
        {
            return source.Select(input => FromArray(input, Depth.U16, 1));
        }

        public IObservable<Seq> Process(IObservable<int[]> source)
        {
            return source.Select(input => FromArray(input, Depth.S32, 1));
        }

        public IObservable<Seq> Process(IObservable<float[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F32, 1));
        }

        public IObservable<Seq> Process(IObservable<double[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F64, 1));
        }

        public IObservable<Seq> Process(IObservable<Point[]> source)
        {
            return source.Select(input => FromArray(input, Depth.S32, 2));
        }

        public IObservable<Seq> Process(IObservable<Point2f[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F32, 2));
        }

        public IObservable<Seq> Process(IObservable<Point2d[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F64, 2));
        }

        public IObservable<Seq> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                var storage = new MemStorage();
                var output = new Seq(input.Depth, input.Channels, SequenceKind.Curve, Flags, storage);
                output.Insert(0, (Arr)input);
                return output;
            });
        }

        public IObservable<Seq> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var storage = new MemStorage();
                var output = new Seq(ArrHelper.FromIplDepth(input.Depth), input.Channels, SequenceKind.Curve, Flags, storage);
                output.Insert(0, (Arr)input);
                return output;
            });
        }

        class FlagsConverter : EnumConverter
        {
            public FlagsConverter(Type type)
                : base(type)
            {
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
            {
                var flags = (SequenceFlags)value;
                if (flags == SequenceFlags.Simple) return "Simple";
                else
                {
                    var closed = (flags & SequenceFlags.Closed) != 0;
                    var hole = (flags & SequenceFlags.Hole) != 0;
                    if (closed && hole) return "Closed, Hole";
                    else if (closed) return "Closed";
                    else if (hole) return "Hole";
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[] { SequenceFlags.Simple, SequenceFlags.Closed, SequenceFlags.Hole });
            }
        }
    }
}
