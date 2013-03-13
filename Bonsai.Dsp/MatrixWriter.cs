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
    public class MatrixWriter : StreamSink<CvMat, BinaryWriter>
    {
        public MatrixLayout Layout { get; set; }

        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        protected override void Write(BinaryWriter writer, CvMat input)
        {
            var data = new byte[input.Step * input.Rows];
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                CvMat dataHeader;
                switch (Layout)
                {
                    case MatrixLayout.ColumnMajor:
                        dataHeader = new CvMat(input.Cols, input.Rows, input.Depth, input.NumChannels, dataHandle.AddrOfPinnedObject());
                        Core.cvTranspose(input, dataHeader);
                        break;
                    default:
                    case MatrixLayout.RowMajor:
                        dataHeader = new CvMat(input.Rows, input.Cols, input.Depth, input.NumChannels, dataHandle.AddrOfPinnedObject());
                        Core.cvCopy(input, dataHeader);
                        break;
                }
            }
            finally { dataHandle.Free(); }
            writer.Write(data);
        }
    }
}
