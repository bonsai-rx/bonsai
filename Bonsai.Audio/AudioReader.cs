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
    [DefaultProperty("FileName")]
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

        IEnumerable<Mat> CreateReader(double bufferLength)
        {
            using (var reader = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
            {
                RiffHeader header;
                RiffReader.ReadHeader(reader, out header);

                var sampleCount = header.DataLength / header.BlockAlign;
                var depth = header.BitsPerSample == 8 ? Depth.U8 : Depth.S16;
                var bufferSize = (int)Math.Ceiling(header.SampleRate * bufferLength / 1000);
                bufferSize = bufferSize <= 0 ? (int)sampleCount : bufferSize;

                var sampleData = new byte[bufferSize * header.BlockAlign];
                for (int i = 0; i < sampleCount / bufferSize; i++)
                {
                    var bytesRead = reader.Read(sampleData, 0, sampleData.Length);
                    if (bytesRead < sampleData.Length) break;

                    var output = new Mat(header.Channels, bufferSize, depth, 1);
                    using (var bufferHeader = Mat.CreateMatHeader(sampleData, bufferSize, header.Channels, depth, 1))
                    {
                        CV.Transpose(bufferHeader, output);
                    }

                    yield return output;
                }
            }
        }

        public override IObservable<Mat> Generate()
        {
            return Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var i = 1L;
                    var bufferLength = BufferLength;
                    using (var reader = CreateReader(bufferLength).GetEnumerator())
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        while (!cancellationToken.IsCancellationRequested)
                        {
                            if (!reader.MoveNext()) break;
                            observer.OnNext(reader.Current);

                            var sampleInterval = (int)(bufferLength * i - stopwatch.ElapsedMilliseconds);
                            if (sampleInterval > 0)
                            {
                                sampleSignal.WaitOne(sampleInterval);
                            }

                            i++;
                        }

                        observer.OnCompleted();
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Zip(CreateReader(BufferLength), (x, output) => output);
        }
    }
}
