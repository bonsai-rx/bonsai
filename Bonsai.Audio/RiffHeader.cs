﻿using OpenTK.Audio.OpenAL;
using System;

namespace Bonsai.Audio
{
    struct RiffHeader
    {
        internal const string RiffId = "RIFF";
        internal const string WaveId = "WAVE";
        internal const string FmtId = "fmt ";
        internal const string DataId = "data";

        public ushort Channels;
        public uint SampleRate;
        public uint ByteRate;
        public ushort BlockAlign;
        public ushort BitsPerSample;
        public uint DataLength;

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
