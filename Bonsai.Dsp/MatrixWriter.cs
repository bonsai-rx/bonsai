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
    public class MatrixWriter : FileSink<CvMat, BinaryWriter>
    {
        protected override BinaryWriter CreateWriter(string fileName, CvMat input)
        {
            var stream = new FileStream(fileName, FileMode.Create);
            var writer = new BinaryWriter(stream);
            return writer;
        }

        protected override void Write(BinaryWriter writer, CvMat input)
        {
            var data = new byte[input.Step * input.Rows];
            Marshal.Copy(input.Data, data, 0, data.Length);
            writer.Write(data);
        }
    }
}
