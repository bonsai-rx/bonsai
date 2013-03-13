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

namespace Bonsai.Vision
{
    public class ImageWriter : StreamSink<IplImage, BinaryWriter>
    {
        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        protected override void Write(BinaryWriter writer, IplImage input)
        {
            var data = new byte[input.WidthStep * input.Height];
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataHeader = new IplImage(input.Size, input.Depth, input.NumChannels, dataHandle.AddrOfPinnedObject());
                Core.cvCopy(input, dataHeader);
            }
            finally { dataHandle.Free(); }
            writer.Write(data);
        }
    }
}
