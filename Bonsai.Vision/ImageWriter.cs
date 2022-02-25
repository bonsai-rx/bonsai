using System;
using OpenCV.Net;
using System.IO;
using Bonsai.IO;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that writes each image in the sequence to the
    /// specified stream.
    /// </summary>
    [Description("Writes each image in the sequence to the specified stream.")]
    public class ImageWriter : StreamSink<IplImage, BinaryWriter>
    {
        /// <summary>
        /// Creates the <see cref="BinaryWriter"/> object that will be responsible
        /// for writing the image data to the specified <see cref="Stream"/>.
        /// </summary>
        /// <param name="stream">The stream on which the image data should be written.</param>
        /// <returns>
        /// The <see cref="BinaryWriter"/> object that will be used to write image
        /// data into the stream.
        /// </returns>
        protected override BinaryWriter CreateWriter(Stream stream)
        {
            return new BinaryWriter(stream);
        }

        /// <summary>
        /// Writes a new image to the binary output stream.
        /// </summary>
        /// <param name="writer">
        /// A <see cref="BinaryWriter"/> object used to write raw image data to
        /// the output stream.
        /// </param>
        /// <param name="input">
        /// The image frame containing the raw binary data to write into the output
        /// stream.
        /// </param>
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
