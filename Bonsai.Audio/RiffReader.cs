using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    static class RiffReader
    {
        public static void ReadHeader(BinaryReader reader, out RiffHeader header)
        {
            var id = new byte[4];
            CheckId(reader, id, RiffHeader.RiffId);
            reader.ReadInt32();
            CheckId(reader, id, RiffHeader.WaveId);
            CheckId(reader, id, RiffHeader.FmtId);

            AssertFormatValue(16u, reader.ReadUInt32());
            AssertFormatValue(1u, reader.ReadUInt16());

            header.Channels = (int)reader.ReadUInt16();
            header.SampleRate = (long)reader.ReadUInt32();
            header.ByteRate = (long)reader.ReadUInt32();
            header.BlockAlign = (int)reader.ReadUInt16();
            header.BitsPerSample = (int)reader.ReadUInt16();

            FindId(reader, id, RiffHeader.DataId);
            header.DataLength = (long)reader.ReadUInt32();
        }

        static void FindId(BinaryReader reader, byte[] bytes, string id)
        {
            while (true)
            {
                var count = reader.Read(bytes, 0, bytes.Length);
                if (count < bytes.Length)
                {
                    var message = string.Format("No {0} chunk found in the specified WAV file.", id);
                    throw new InvalidOperationException(message);
                }

                if (string.Compare(Encoding.ASCII.GetString(bytes), id, true) != 0)
                {
                    var size = reader.ReadUInt32();
                    reader.BaseStream.Seek(size, SeekOrigin.Current);
                }
                else break;
            }
        }

        static void CheckId(BinaryReader reader, byte[] bytes, string id)
        {
            var count = reader.Read(bytes, 0, bytes.Length);
            if (count < bytes.Length ||
                string.Compare(Encoding.ASCII.GetString(bytes), id, true) != 0)
            {
                throw new InvalidOperationException("The specified file has an invalid RIFF header.");
            }
        }

        static void AssertFormatValue(uint expected, uint actual)
        {
            if (expected != actual)
            {
                throw new InvalidOperationException("The specified file has an unsupported WAV format.");
            }
        }
    }
}
