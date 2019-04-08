using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;
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
    [DefaultProperty("Path")]
    [Description("Sources buffered signal samples from the specified raw binary input stream.")]
    public class MatrixReader : Source<Mat>
    {
        const string PipePathPrefix = @"\\";
        static readonly Regex PipePathRegex = new Regex(@"\\\\([^\\]*)\\pipe\\(\w+)");

        public MatrixReader()
        {
            ChannelCount = 1;
        }

        [Description("The name of the input data path.")]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string Path { get; set; }

        [Browsable(false)]
        public string FileName
        {
            get { return Path; }
            set { Path = value; }
        }

        [Description("The byte offset at which to start reading the raw binary file.")]
        public long Offset { get; set; }

        [Description("The sample rate of the stored signal, in Hz.")]
        public int SampleRate { get; set; }

        [Browsable(false)]
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

        [Browsable(false)]
        public bool FrequencySpecified
        {
            get { return Frequency.HasValue; }
        }

        [Description("The number of channels in the stored signal.")]
        public int ChannelCount { get; set; }

        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; }

        [Description("The bit depth of individual buffer elements.")]
        public Depth Depth { get; set; }

        [Description("The sequential memory layout used to store the sample buffers.")]
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
            using (var reader = new BinaryReader(CreateStream(FileName)))
            {
                var depth = Depth;
                var channelCount = ChannelCount;
                var offset = Offset;
                if (offset > 0)
                {
                    reader.BaseStream.Seek(offset, SeekOrigin.Begin);
                }

                var bufferLength = BufferLength;
                if (bufferLength == 0)
                {
                    bufferLength = (int)(reader.BaseStream.Length - offset);
                }

                byte[] buffer = null;
                while (true)
                {
                    var output = new Mat(channelCount, bufferLength, depth, 1);
                    var depthSize = output.Step / bufferLength;
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

        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Zip(CreateReader(), (x, output) => output);
        }
    }
}
