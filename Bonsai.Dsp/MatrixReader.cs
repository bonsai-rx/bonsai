using System;
using System.Collections.Generic;
using OpenCV.Net;
using System.ComponentModel;
using System.IO;
using System.Reactive.Linq;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.IO.Pipes;
using System.Text.RegularExpressions;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that generates a sequence of signal sample buffers from
    /// the specified raw binary input stream.
    /// </summary>
    [DefaultProperty(nameof(Path))]
    [Description("Generates a sequence of signal sample buffers from the specified raw binary input stream.")]
    public class MatrixReader : Source<Mat>
    {
        const string PipePathPrefix = @"\\";
        static readonly Regex PipePathRegex = new Regex(@"\\\\([^\\]*)\\pipe\\(\w+)");

        /// <summary>
        /// Gets or sets the identifier of the named stream from which to read the samples.
        /// </summary>
        /// <remarks>
        /// If the identifier uses the named pipe prefix <c>\\.\pipe\</c>, a corresponding
        /// <see cref="NamedPipeClientStream"/> object is created; otherwise a regular
        /// <see cref="FileStream"/> is used.
        /// </remarks>
        [Description("The identifier of the named stream from which to read the samples.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        public string Path { get; set; }

        /// <summary>
        /// Gets or sets the name of the file from which to read the samples.
        /// </summary>
        [Browsable(false)]
        [Obsolete("Use Path instead for the option to read binary data from named pipes.")]
        public string FileName
        {
            get { return Path; }
            set { Path = value; }
        }

        /// <summary>
        /// Gets or sets the byte offset at which to start reading the raw binary file.
        /// </summary>
        [Description("The byte offset at which to start reading the raw binary file.")]
        public long Offset { get; set; }

        /// <summary>
        /// Gets or sets the sample rate of the stored signal, in Hz.
        /// </summary>
        [Description("The sample rate of the stored signal, in Hz.")]
        public int SampleRate { get; set; }

        /// <summary>
        /// Gets or sets the sampling rate of the generated signal waveform, in Hz.
        /// </summary>
        [Browsable(false)]
        [Obsolete("Use SampleRate instead for consistent wording with signal processing operator properties.")]
        public int? Frequency
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    SampleRate = value.Value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Frequency"/> property should be serialized.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public bool FrequencySpecified
        {
            get { return Frequency.HasValue; }
        }

        /// <summary>
        /// Gets or sets the number of channels in the stored signal.
        /// </summary>
        [Description("The number of channels in the stored signal.")]
        public int ChannelCount { get; set; } = 1;

        /// <summary>
        /// Gets or sets the number of samples in each output buffer.
        /// </summary>
        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; }

        /// <summary>
        /// Gets or sets the bit depth of each element in an output buffer.
        /// </summary>
        [Description("The bit depth of each element in an output buffer.")]
        public Depth Depth { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the sequential memory layout used to
        /// store the sample buffers.
        /// </summary>
        [Description("Specifies the sequential memory layout used to store the sample buffers.")]
        public MatrixLayout Layout { get; set; }

        static Stream CreateStream(string path)
        {
            if (path.StartsWith(PipePathPrefix))
            {
                var match = PipePathRegex.Match(path);
                if (match.Success)
                {
                    var serverName = match.Groups[1].Value;
                    var pipeName = match.Groups[2].Value;
                    var stream = new NamedPipeClientStream(serverName, pipeName, PipeDirection.In);
                    try { stream.Connect(); }
                    catch { stream.Close(); throw; }
                    return stream;
                }
            }
            
            return File.OpenRead(path);
        }

        IEnumerable<Mat> CreateReader()
        {
            using (var reader = new BinaryReader(CreateStream(Path)))
            {
                var depth = Depth;
                var channelCount = ChannelCount;
                var depthSize = ArrHelper.ElementSize(depth);
                var offset = Offset;
                if (offset > 0)
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                }

                var bufferLength = BufferLength;
                if (bufferLength == 0)
                {
                    bufferLength = (int)(reader.BaseStream.Length - offset) / depthSize;
                }

                byte[] buffer = null;
                while (true)
                {
                    var output = new Mat(channelCount, bufferLength, depth, 1);
                    buffer = buffer ?? new byte[bufferLength * channelCount * depthSize];
                    var bytesRead = reader.Read(buffer, 0, buffer.Length);
                    if (bytesRead < buffer.Length) yield break;

                    Mat bufferHeader;
                    var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                    try
                    {
                        switch (Layout)
                        {
                            case MatrixLayout.ColumnMajor:
                                bufferHeader = new Mat(bufferLength, channelCount, depth, 1, bufferHandle.AddrOfPinnedObject());
                                CV.Transpose(bufferHeader, output);
                                break;
                            default:
                            case MatrixLayout.RowMajor:
                                bufferHeader = new Mat(channelCount, bufferLength, depth, 1, bufferHandle.AddrOfPinnedObject());
                                CV.Copy(bufferHeader, output);
                                break;
                        }
                    }
                    finally { bufferHandle.Free(); }
                    yield return output;
                }
            }
        }

        /// <summary>
        /// Generates an observable sequence of signal sample buffers from
        /// the specified raw binary input stream.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing fixed-size buffers
        /// of samples from the signal stored in the specified file.
        /// </returns>
        public override IObservable<Mat> Generate()
        {
            return Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    using (var reader = CreateReader().GetEnumerator())
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        var stopwatch = new Stopwatch();
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            stopwatch.Restart();
                            if (!reader.MoveNext()) break;
                            observer.OnNext(reader.Current);

                            var sampleRate = SampleRate;
                            var sampleInterval = sampleRate > 0 ? 1000.0 / sampleRate : 0;
                            var dueTime = Math.Max(0, (sampleInterval * BufferLength) - stopwatch.Elapsed.TotalMilliseconds);
                            if (dueTime > 0)
                            {
                                sampleSignal.WaitOne(TimeSpan.FromMilliseconds(dueTime));
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

        /// <summary>
        /// Generates an observable sequence of signal sample buffers from
        /// the specified raw binary input stream, where each new buffer is
        /// emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting sample buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing fixed-size buffers
        /// of samples from the signal stored in the specified file.
        /// </returns>
        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Zip(CreateReader(), (x, output) => output);
        }
    }
}
