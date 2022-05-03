using Bonsai.IO;
using OpenCV.Net;
using System;
using System.ComponentModel;
using System.IO;
using System.Runtime.InteropServices;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that writes a sequence of buffered samples into an uncompressed RIFF/WAV file.
    /// </summary>
    [Description("Writes a sequence of buffered samples into an uncompressed RIFF/WAV file.")]
    public class AudioWriter : FileSink<Mat, RiffWriter>
    {
        /// <summary>
        /// Gets or sets the sample rate of the input signal, in Hz.
        /// </summary>
        [Description("The sample rate of the input signal, in Hz.")]
        public int SampleRate { get; set; } = 44100;

        /// <summary>
        /// Gets or sets the sample rate of the input signal, in Hz.
        /// </summary>
        [Browsable(false)]
        [Obsolete("Use SampleRate instead for consistent wording with signal processing operator properties.")]
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

        /// <summary>
        /// Gets a value indicating whether the <see cref="SamplingFrequency"/> property should be serialized.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public bool SamplingFrequencySpecified
        {
            get { return SamplingFrequency.HasValue; }
        }

        /// <inheritdoc/>
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

        /// <summary>
        /// Writes a sample buffer into the WAV file.
        /// </summary>
        /// <param name="writer">The <see cref="RiffWriter"/> used to write data into the WAV file.</param>
        /// <param name="input">
        /// A <see cref="Mat"/> object containing the audio samples to write into the file.
        /// </param>
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
