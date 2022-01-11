using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that converts a fixed size array type into a
    /// sequence of elements.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts a fixed size array type into a sequence of elements.")]
    public class ConvertToSeq
    {
        /// <summary>
        /// Gets or sets the operation flags for the element sequence.
        /// </summary>
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

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of 8-bit unsigned integer arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<byte[]> source)
        {
            return source.Select(input => FromArray(input, Depth.U8, 1));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of 16-bit signed integer arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<short[]> source)
        {
            return source.Select(input => FromArray(input, Depth.S16, 1));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of 16-bit unsigned integer arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<ushort[]> source)
        {
            return source.Select(input => FromArray(input, Depth.U16, 1));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of 32-bit signed integer arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<int[]> source)
        {
            return source.Select(input => FromArray(input, Depth.S32, 1));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of 32-bit floating-point arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<float[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F32, 1));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of 64-bit floating-point arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<double[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F64, 1));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of <see cref="Point"/> arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<Point[]> source)
        {
            return source.Select(input => FromArray(input, Depth.S32, 2));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of <see cref="Point2f"/> arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<Point2f[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F32, 2));
        }

        /// <summary>
        /// Converts each fixed size array in an observable sequence into a
        /// growable sequence of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of <see cref="Point2d"/> arrays to convert into
        /// a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the array data.
        /// </returns>
        public IObservable<Seq> Process(IObservable<Point2d[]> source)
        {
            return source.Select(input => FromArray(input, Depth.F64, 2));
        }

        /// <summary>
        /// Converts each 2D array in an observable sequence into a growable sequence
        /// of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of multi-channel matrices to convert into a
        /// growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the multi-channel matrix data.
        /// </returns>
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

        /// <summary>
        /// Converts each image in an observable sequence into a growable sequence
        /// of elements.
        /// </summary>
        /// <param name="source">
        /// An observable sequence of images to convert into a growable sequence of elements.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Seq"/> objects representing the growable sequence
        /// header for the image data.
        /// </returns>
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
