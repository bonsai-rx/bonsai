using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    [Description("Sources buffered audio samples from an uncompressed RIFF/WAV file.")]
    public class AudioReader : Source<Mat>
    {
        public AudioReader()
        {
            BufferLength = 10;
        }

        [Description("The name of the WAV file.")]
        [FileNameFilter("WAV Files (*.wav;*.wave)|*.wav;*.wave|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        [Description("The length of the sample buffer (ms).")]
        public double BufferLength { get; set; }

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

        public override IObservable<Mat> Generate()
        {
            return Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using (var reader = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        var id = new byte[4];
                        CheckId(reader, id, RiffHeader.RiffId);
                        reader.ReadInt32();
                        CheckId(reader, id, RiffHeader.WaveId);
                        CheckId(reader, id, RiffHeader.FmtId);

                        AssertFormatValue(16u, reader.ReadUInt32());
                        AssertFormatValue(1u, reader.ReadUInt16());

                        var channels = (int)reader.ReadUInt16();
                        var samplingFrequency = (long)reader.ReadUInt32();
                        reader.ReadUInt32();
                        var sampleSize = (int)reader.ReadUInt16();
                        var depth = reader.ReadUInt16() == 8 ? Depth.U8 : Depth.S16;

                        var bufferLength = BufferLength;
                        var bufferSize = (int)Math.Ceiling(samplingFrequency * bufferLength / 1000);
                        CheckId(reader, id, RiffHeader.DataId);
                        var dataSize = (long)reader.ReadUInt32();
                        var sampleCount = dataSize / sampleSize;

                        var stopwatch = new Stopwatch();
                        var sampleData = new byte[bufferSize * sampleSize];
                        for (int i = 0; i < sampleCount / bufferSize; i++)
                        {
                            stopwatch.Restart();
                            if (cancellationToken.IsCancellationRequested) break;
                            var bytesRead = reader.Read(sampleData, 0, sampleData.Length);
                            if (bytesRead < sampleData.Length) break;

                            var output = new Mat(channels, bufferSize, depth, 1);
                            using (var bufferHeader = Mat.CreateMatHeader(sampleData, bufferSize, channels, depth, 1))
                            {
                                CV.Transpose(bufferHeader, output);
                            }

                            observer.OnNext(output);
                            var sampleInterval = (int)(bufferLength - stopwatch.ElapsedMilliseconds);
                            if (sampleInterval > 0)
                            {
                                sampleSignal.WaitOne(sampleInterval);
                            }
                        }

                        observer.OnCompleted();
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
