using Bonsai.IO;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [Description("Writes the incoming sample sequence into an uncompressed RIFF/WAV file.")]
    public class AudioWriter : FileSink<Mat, RiffWriter>
    {
        public AudioWriter()
        {
            SampleRate = 44100;
        }

        [Description("The sample rate of the input signal, in Hz.")]
        public int SampleRate { get; set; }

        [Browsable(false)]
        public double? SamplingFrequency
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    SampleRate = (int)value.Value;
                }
            }
        }

        [Browsable(false)]
        public bool SamplingFrequencySpecified
        {
            get { return SamplingFrequency.HasValue; }
        }

        protected override RiffWriter CreateWriter(string fileName, Mat input)
        {
            var sampleRate = SampleRate;
            if (sampleRate <= 0)
            {
                throw new InvalidOperationException("Sample rate must be a positive integer.");
            }

            var stream = new FileStream(fileName, FileMode.Create);
            var bitsPerSample = input.Depth == Depth.U8 ? 8 : 16;
            return new RiffWriter(stream, input.Rows, sampleRate, bitsPerSample);
        }

        protected override void Write(RiffWriter writer, Mat input)
        {
            var dataDepth = input.Depth == Depth.U8 ? Depth.U8 : Depth.S16;
            var elementSize = input.Depth == Depth.U8 ? 1 : 2;
            var step = elementSize * input.Cols;
            var data = new byte[step * input.Rows];
            var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
            try
            {
                var dataAddr = dataHandle.AddrOfPinnedObject();
                using (var dataHeader = new Mat(input.Cols, input.Rows, dataDepth, input.Channels,
                                                dataAddr, elementSize * input.Rows))
                {
                    if (input.Depth != dataDepth)
                    {
                        using (var conversion = new Mat(input.Size, dataDepth, input.Channels))
                        {
                            CV.Convert(input, conversion);
                            CV.Transpose(conversion, dataHeader);
                        }
                    }
                    else CV.Transpose(input, dataHeader);
                }
            }
            finally { dataHandle.Free(); }
            writer.Write(data);
        }
    }
}
