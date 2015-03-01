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
    [Description("Writes the incoming signal into a raw binary file.")]
    public class MatrixWriter : StreamSink<Mat, BinaryWriter>
    {
        public MatrixWriter()
        {
            Layout = MatrixLayout.ColumnMajor;
        }

        [Description("The memory layout used to store the signal on disk.")]
        public MatrixLayout Layout { get; set; }

        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        protected override void Write(BinaryWriter writer, Mat input)
        {
            var data = ArrHelper.ToArray(input, Layout);
            writer.Write(data);
        }
    }
}
