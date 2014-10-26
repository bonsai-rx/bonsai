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

namespace Bonsai.Dsp
{
    public class MatrixReader : Source<Mat>
    {
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public int Frequency { get; set; }

        public int ChannelCount { get; set; }

        public int BufferLength { get; set; }

        public Depth Depth { get; set; }

        public MatrixLayout Layout { get; set; }

        public override IObservable<Mat> Generate()
        {
            var stopwatch = new Stopwatch();
            return Observable.Using(
                () => new BinaryReader(new FileStream(FileName, FileMode.Open, FileAccess.Read)),
                reader => ObservableCombinators.GenerateWithThread<Mat>(observer =>
                {
                    stopwatch.Restart();
                    var channelCount = ChannelCount;
                    var bufferLength = BufferLength;
                    var output = new Mat(channelCount, bufferLength, Depth, 1);
                    var depthSize = output.Step / bufferLength;
                    var buffer = new byte[bufferLength * channelCount * depthSize];
                    var bytesRead = reader.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0)
                    {
                        observer.OnCompleted();
                        return;
                    }
                    else
                    {
                        Mat bufferHeader;
                        var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        try
                        {
                            switch (Layout)
                            {
                                case MatrixLayout.ColumnMajor:
                                    bufferHeader = new Mat(bufferLength, channelCount, Depth, 1, bufferHandle.AddrOfPinnedObject());
                                    CV.Transpose(bufferHeader, output);
                                    break;
                                default:
                                case MatrixLayout.RowMajor:
                                    bufferHeader = new Mat(channelCount, bufferLength, Depth, 1, bufferHandle.AddrOfPinnedObject());
                                    CV.Copy(bufferHeader, output);
                                    break;
                            }
                        }
                        finally { bufferHandle.Free(); }

                        observer.OnNext(output);
                        var sampleInterval = 1000.0 / Frequency;
                        var dueTime = Math.Max(0, (sampleInterval * BufferLength) - stopwatch.Elapsed.TotalMilliseconds);
                        if (dueTime > 0)
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(dueTime));
                        }
                    }
                }));
        }
    }
}
