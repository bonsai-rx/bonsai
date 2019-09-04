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
    [Description("Writes the incoming image sequence to the specified stream.")]
    public class ImageWriter : StreamSink<IplImage, BinaryWriter>
    {
        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        protected override void Write(BinaryWriter writer, IplImage input)
        {
            var step = input.Width * input.Channels * ((int)(input.Depth) & 0xFF) / 8;
            var data = new byte[step * input.Height];
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataHeader = new IplImage(input.Size, input.Depth, input.Channels, IntPtr.Zero);
                dataHeader.SetData(dataHandle.AddrOfPinnedObject(), step);
                CV.Copy(input, dataHeader);
            }
            finally { dataHandle.Free(); }
            writer.Write(data);
        }
    }
}
