using System;
using OpenCV.Net;
using System.IO;
using Bonsai.IO;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that writes each array-like object in the sequence
    /// to a raw binary output stream.
    /// </summary>
    [Description("Writes each array-like object in the sequence to a raw binary output stream.")]
    public class MatrixWriter : StreamSink<ArraySegment<byte>, BinaryWriter>
    {
        /// <summary>
        /// Gets or sets a value specifying the sequential memory layout used to
        /// store the sample buffers.
        /// </summary>
        [Description("Specifies the sequential memory layout used to store the sample buffers.")]
        public MatrixLayout Layout { get; set; } = MatrixLayout.ColumnMajor;

        /// <summary>
        /// Creates a binary writer over the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream on which the elements should be written.</param>
        /// <returns>
        /// A <see cref="BinaryWriter"/> object used to write binary array data
        /// into the stream.
        /// </returns>
        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        /// <summary>
        /// Writes a new array to the raw binary output stream.
        /// </summary>
        /// <param name="writer">
        /// A <see cref="BinaryWriter"/> object used to write binary array data to
        /// the output stream.
        /// </param>
        /// <param name="input">
        /// The array segment containing the binary data to write into the output
        /// stream.
        /// </param>
        protected override void Write(BinaryWriter writer, ArraySegment<byte> input)
        {
            writer.Write(input.Array, input.Offset, input.Count);
        }

        /// <summary>
        /// Writes all of the arrays in an observable sequence to the specified raw binary output stream.
        /// </summary>
        /// <typeparam name="TElement">
        /// The type of the elements in each array. This type must be a non-pointer, non-nullable
        /// unmanaged type.
        /// </typeparam>
        /// <param name="source">The sequence of arrays to write.</param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the arrays to a binary stream.
        /// </returns>
        public unsafe IObservable<TElement[]> Process<TElement>(IObservable<TElement[]> source) where TElement : unmanaged
        {
            return Process(source, input =>
            {
                var bytes = new byte[input.Length * sizeof(TElement)];
                System.Buffer.BlockCopy(input, 0, bytes, 0, bytes.Length);
                return new ArraySegment<byte>(bytes);
            });
        }

        /// <summary>
        /// Writes all of the <see cref="byte"/> arrays in an observable sequence to the
        /// specified raw binary output stream.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="byte"/> arrays to write.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the arrays to a stream.
        /// </returns>
        public IObservable<byte[]> Process(IObservable<byte[]> source)
        {
            return Process(source, input => new ArraySegment<byte>(input));
        }

        /// <summary>
        /// Writes all multi-channel matrices in an observable sequence to the
        /// specified raw binary output stream.
        /// </summary>
        /// <param name="source">
        /// The sequence of multi-channel matrices to write.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of writing the matrices to a stream.
        /// </returns>
        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Process(source, input => new ArraySegment<byte>(ArrHelper.ToArray(input, Layout)));
        }
    }
}
