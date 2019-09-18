using OpenTK.Audio.OpenAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    struct RiffHeader
    {
        internal const string RiffId = "RIFF";
        internal const string WaveId = "WAVE";
        internal const string FmtId = "fmt ";
        internal const string DataId = "data";

        public int Channels;
        public long SampleRate;
        public long ByteRate;
        public int BlockAlign;
        public int BitsPerSample;
        public long DataLength;

        public ALFormat GetFormat()
        {
            ALFormat format;
            switch (BitsPerSample)
            {
                case 8: format = ALFormat.Mono8; break;
                case 16: format = ALFormat.Mono16; break;
                default: throw new InvalidOperationException("The WAV format is incompatible with OpenAL.");
            }

            switch (Channels)
            {
                case 1: break;
                case 2: format += 2; break;
                default: throw new InvalidOperationException("The number of channels in the WAV file is incompatible with OpenAL.");
            }

            return format;
        }
    }
}
