using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    public class RiffWriter : IDisposable
    {
        bool disposed;
        long dataSize;
        WaveFormatEx format;
        readonly BinaryWriter writer;
        static readonly byte[] riffId = Encoding.ASCII.GetBytes(RiffHeader.RiffId);
        static readonly byte[] waveId = Encoding.ASCII.GetBytes(RiffHeader.WaveId);
        static readonly byte[] fmtId = Encoding.ASCII.GetBytes(RiffHeader.FmtId);
        static readonly byte[] dataId = Encoding.ASCII.GetBytes(RiffHeader.DataId);
        
        public RiffWriter(Stream stream, int channels, int samplesPerSecond, int bitsPerSample)
        {
            format.wFormatTag = 1;
            format.nChannels = (ushort)channels;
            format.nSamplesPerSec = (uint)samplesPerSecond;
            format.nAvgBytesPerSec = (uint)(channels * samplesPerSecond * bitsPerSample / 8);
            format.nBlockAlign = (ushort)(channels * bitsPerSample / 8);
            format.wBitsPerSample = (ushort)bitsPerSample;

            writer = new BinaryWriter(stream);
            WriteRiffHeader(writer, format, 0);
        }

        static void WriteRiffHeader(BinaryWriter writer, WaveFormatEx format, uint dataSize)
        {
            var formatSize = (uint)Marshal.SizeOf(typeof(WaveFormatEx));
            var totalSize = (uint)
            (
                formatSize + dataSize +
                waveId.Length + fmtId.Length + dataId.Length +
                4 + 4
            );

            writer.Write(riffId);
            writer.Write(totalSize);
            writer.Write(waveId);
            writer.Write(fmtId);
            writer.Write(formatSize);

            writer.Write(format.wFormatTag);
            writer.Write(format.nChannels);
            writer.Write(format.nSamplesPerSec);
            writer.Write(format.nAvgBytesPerSec);
            writer.Write(format.nBlockAlign);
            writer.Write(format.wBitsPerSample);
            writer.Write(dataId);
            writer.Write(dataSize);
        }

        public void Write(byte[] buffer)
        {
            writer.Write(buffer);
            dataSize += buffer.LongLength;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    var position = writer.BaseStream.Position;
                    writer.BaseStream.Seek(0, SeekOrigin.Begin);
                    WriteRiffHeader(writer, format, (uint)dataSize);
                    writer.BaseStream.Seek(position, SeekOrigin.Begin);
                    writer.Close();
                    disposed = true;
                }
            }
        }

        public void Close()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        void IDisposable.Dispose()
        {
            Close();
        }

        #region WaveFormatEx Structure

        [StructLayout(LayoutKind.Sequential)]
        struct WaveFormatEx
        {
            public ushort wFormatTag;
            public ushort nChannels;
            public uint nSamplesPerSec;
            public uint nAvgBytesPerSec;
            public ushort nBlockAlign;
            public ushort wBitsPerSample;
        }

        #endregion
    }
}
