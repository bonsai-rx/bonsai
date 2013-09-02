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
    public class MatrixWriter : StreamSink<Mat, BinaryWriter>
    {
        public MatrixLayout Layout { get; set; }

        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        protected override void Write(BinaryWriter writer, Mat input)
        {
            var data = new byte[input.Step * input.Rows];
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                Mat dataHeader;
                switch (Layout)
                {
                    case MatrixLayout.ColumnMajor:
                        dataHeader = new Mat(input.Cols, input.Rows, input.Depth, input.Channels, dataHandle.AddrOfPinnedObject());
                        CV.Transpose(input, dataHeader);
                        break;
                    default:
                    case MatrixLayout.RowMajor:
                        dataHeader = new Mat(input.Rows, input.Cols, input.Depth, input.Channels, dataHandle.AddrOfPinnedObject());
                        CV.Copy(input, dataHeader);
                        break;
                }
            }
            finally { dataHandle.Free(); }
            writer.Write(data);
        }
    }
}
