using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Threading.Tasks;
using System.IO;
using Bonsai.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Bonsai.Dsp
{
    [Description("Writes the incoming signal into the specified raw binary output stream.")]
    public class MatrixWriter : StreamSink<byte[], BinaryWriter>
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

        protected override void Write(BinaryWriter writer, byte[] input)
        {
            writer.Write(input);
        }

        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Process(source, input => ArrHelper.ToArray(input, Layout));
        }
    }
}
