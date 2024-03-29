﻿using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Audio
{
    /// <summary>
    /// Represents an operator that generates a sequence of buffered audio samples from an
    /// uncompressed RIFF/WAV file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Generates a sequence of buffered audio samples from an uncompressed RIFF/WAV file.")]
    public class AudioReader : Source<Mat>
    {
        /// <summary>
        /// Gets or sets the name of the WAV file.
        /// </summary>
        [Description("The name of the WAV file.")]
        [FileNameFilter("WAV Files (*.wav;*.wave)|*.wav;*.wave|All Files|*.*")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the length of the sample buffer, in milliseconds.
        /// </summary>
        [Description("The length of the sample buffer, in milliseconds.")]
        public double BufferLength { get; set; } = 10;

        /// <summary>
        /// Gets or sets the sample rate, in Hz, used to playback the sample buffers.
        /// If it is zero, samples will be played at the rate specified in the
        /// RIFF/WAV file header.
        /// </summary>
        [Description("The sample rate, in Hz, used to playback the sample buffers.")]
        public int SampleRate { get; set; }

        IEnumerable<Mat> CreateReader(double bufferLength)
        {
            using (var reader = new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)))
            {
                RiffHeader header;
                RiffReader.ReadHeader(reader, out header);

                var sampleRate = SampleRate;
                if (sampleRate <= 0) sampleRate = (int)header.SampleRate;
                var sampleCount = header.DataLength / header.BlockAlign;
                var depth = header.BitsPerSample == 8 ? Depth.U8 : Depth.S16;
                var bufferSize = (int)Math.Ceiling(sampleRate * bufferLength / 1000);
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

        /// <summary>
        /// Generates a sequence of buffered audio samples from the specified WAV file.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing audio sample
        /// buffers of a fixed length. See <see cref="BufferLength"/>.
        /// </returns>
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

        /// <summary>
        /// Generates a sequence of buffered audio samples from the specified WAV file, where
        /// each new buffer is emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting audio buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing audio sample
        /// buffers of a fixed length. See <see cref="BufferLength"/>.
        /// </returns>
        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Zip(CreateReader(BufferLength), (x, output) => output);
        }
    }
}
