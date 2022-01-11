using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that concatenates each set of arrays in the sequence
    /// into a single buffer.
    /// </summary>
    [Description("Concatenates each set of arrays in the sequence into a single buffer.")]
    public class Concat : BinaryArrayTransform
    {
        /// <summary>
        /// Gets or sets the dimension along which to merge the arrays.
        /// </summary>
        /// <remarks>
        /// A value of zero specifies concatenating rows, and a value of one specifies
        /// concatenating columns. In the case of concatenating single-dimension arrays,
        /// the <see cref="Axis"/> property is ignored.
        /// </remarks>
        [Description("The dimension along which to merge the arrays.")]
        public int Axis { get; set; }

        TElement[] Process<TElement>(IEnumerable<TElement[]> sources)
        {
            var length = 0;
            foreach (var source in sources)
            {
                length += source.Length;
            }

            var offset = 0;
            var output = new TElement[length];
            foreach (var source in sources)
            {
                Array.Copy(source, 0, output, offset, source.Length);
                offset += source.Length;
            }
            return output;
        }

        TArray Process<TArray>(IEnumerable<TArray> sources) where TArray : Arr
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            var axis = Axis;

            TArray first = null;
            int elementType = 0;
            int elementWidth = 0;
            int elementHeight = 0;
            int outputWidth = 0;
            int outputHeight = 0;
            foreach (var source in sources)
            {
                var size = source.Size;
                if (first == null)
                {
                    first = source;
                    elementType = first.ElementType;
                    elementWidth = size.Width;
                    elementHeight = size.Height;
                    outputWidth = axis == 0 ? size.Width : 0;
                    outputHeight = axis == 1 ? size.Height : 0;
                }

                if (source.ElementType != elementType)
                {
                    throw new InvalidOperationException("Input arrays must have the same element type.");
                }

                if (axis == 0 && size.Width != elementWidth ||
                    axis == 1 && size.Height != elementHeight)
                {
                    throw new InvalidOperationException("Input arrays must have the same shape except in the dimension corresponding to axis.");
                }

                outputWidth += axis == 1 ? size.Width : 0;
                outputHeight += axis == 0 ? size.Height : 0;
            }

            int rowOffset = 0;
            int columnOffset = 0;
            var output = outputFactory(first, new Size(outputWidth, outputHeight));
            foreach (var source in sources)
            {
                var size = source.Size;
                using (var subRect = output.GetSubRect(new Rect(columnOffset, rowOffset, size.Width, size.Height)))
                {
                    CV.Copy(source, subRect);
                }

                rowOffset += axis == 0 ? size.Height : 0;
                columnOffset += axis == 1 ? size.Width : 0;
            }

            return output;
        }

        /// <summary>
        /// Concatenates each pair of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of arrays to concatenate into a single buffer.
        /// </param>
        /// <returns>
        /// The sequence of concatenated buffers.
        /// </returns>
        public override IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2 }));
        }

        /// <summary>
        /// Concatenates each triple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of triples of arrays to concatenate into a single buffer.
        /// </param>
        /// <returns>
        /// The sequence of concatenated buffers.
        /// </returns>
        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3 }));
        }

        /// <summary>
        /// Concatenates each quadruple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of quadruples of arrays to concatenate into a single buffer.
        /// </param>
        /// <returns>
        /// The sequence of concatenated buffers.
        /// </returns>
        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4 }));
        }

        /// <summary>
        /// Concatenates each quintuple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of quintuples of arrays to concatenate into a single buffer.
        /// </param>
        /// <returns>
        /// The sequence of concatenated buffers.
        /// </returns>
        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5 }));
        }

        /// <summary>
        /// Concatenates each sextuple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of sextuples of arrays to concatenate into a single buffer.
        /// </param>
        /// <returns>
        /// The sequence of concatenated buffers.
        /// </returns>
        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6 }));
        }

        /// <summary>
        /// Concatenates each septuple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of septuples of arrays to concatenate into a single buffer.
        /// </param>
        /// <returns>
        /// The sequence of concatenated buffers.
        /// </returns>
        public IObservable<TArray> Process<TArray>(IObservable<Tuple<TArray, TArray, TArray, TArray, TArray, TArray, TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6, input.Item7 }));
        }

        /// <summary>
        /// Concatenates each list of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TArray">
        /// The type of the array-like objects in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of lists of arrays to concatenate into a single buffer.
        /// </param>
        /// <returns>
        /// The sequence of concatenated buffers.
        /// </returns>
        public IObservable<TArray> Process<TArray>(IObservable<IList<TArray>> source) where TArray : Arr
        {
            return source.Select(input => Process(input));
        }

        /// <summary>
        /// Concatenates each pair of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements stored in the array sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of pairs of arrays to concatenate into a single array.
        /// </param>
        /// <returns>
        /// The sequence of concatenated arrays.
        /// </returns>
        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2 }));
        }

        /// <summary>
        /// Concatenates each triple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements stored in the array sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of triples of arrays to concatenate into a single array.
        /// </param>
        /// <returns>
        /// The sequence of concatenated arrays.
        /// </returns>
        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3 }));
        }

        /// <summary>
        /// Concatenates each quadruple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements stored in the array sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of quadruples of arrays to concatenate into a single array.
        /// </param>
        /// <returns>
        /// The sequence of concatenated arrays.
        /// </returns>
        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4 }));
        }

        /// <summary>
        /// Concatenates each quintuple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements stored in the array sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of quintuples of arrays to concatenate into a single array.
        /// </param>
        /// <returns>
        /// The sequence of concatenated arrays.
        /// </returns>
        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5 }));
        }

        /// <summary>
        /// Concatenates each sextuple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements stored in the array sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of sextuples of arrays to concatenate into a single array.
        /// </param>
        /// <returns>
        /// The sequence of concatenated arrays.
        /// </returns>
        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6 }));
        }

        /// <summary>
        /// Concatenates each septuple of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements stored in the array sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of septuples of arrays to concatenate into a single array.
        /// </param>
        /// <returns>
        /// The sequence of concatenated arrays.
        /// </returns>
        public IObservable<TElement[]> Process<TElement>(IObservable<Tuple<TElement[], TElement[], TElement[], TElement[], TElement[], TElement[], TElement[]>> source)
        {
            return source.Select(input => Process(new[] { input.Item1, input.Item2, input.Item3, input.Item4, input.Item5, input.Item6, input.Item7 }));
        }

        /// <summary>
        /// Concatenates each list of arrays in the sequence into a single buffer.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements stored in the array sequence.
        /// </typeparam>
        /// <param name="source">
        /// A sequence of lists of arrays to concatenate into a single array.
        /// </param>
        /// <returns>
        /// The sequence of concatenated arrays.
        /// </returns>
        public IObservable<TElement[]> Process<TElement>(IObservable<IList<TElement[]>> source)
        {
            return source.Select(input => Process(input));
        }
    }
}
