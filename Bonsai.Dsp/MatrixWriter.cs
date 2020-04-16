using System;
using OpenCV.Net;
using System.IO;
using Bonsai.IO;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Description("Writes the incoming signal into the specified raw binary output stream.")]
    public class MatrixWriter : StreamSink<ArraySegment<byte>, BinaryWriter>
    {
        public MatrixWriter()
        {
            Layout = MatrixLayout.ColumnMajor;
        }

        [Description("The sequential memory layout used to store the sample buffers.")]
        public MatrixLayout Layout { get; set; }

        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        protected override void Write(BinaryWriter writer, ArraySegment<byte> input)
        {
            writer.Write(input.Array, input.Offset, input.Count);
        }

        public IObservable<byte[]> Process(IObservable<byte[]> source)
        {
            return Process(source, input => new ArraySegment<byte>(input));
        }

        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Process(source, input => new ArraySegment<byte>(ArrHelper.ToArray(input, Layout)));
        }
    }
}
