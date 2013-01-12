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
    public class MatrixReader : Source<CvMat>
    {
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        public string FileName { get; set; }

        public int Frequency { get; set; }

        public int ChannelCount { get; set; }

        public int BufferLength { get; set; }

        public CvMatDepth Depth { get; set; }

        public MatrixLayout Layout { get; set; }

        protected override IObservable<CvMat> Generate()
        {
            var stopwatch = new Stopwatch();
            return Observable.Using(
                () => new BinaryReader(new FileStream(FileName, FileMode.Open)),
                reader => ObservableCombinators.GenerateWithThread<CvMat>(observer =>
                {
                    stopwatch.Restart();
                    var channelCount = ChannelCount;
                    var bufferLength = BufferLength;
                    var output = new CvMat(channelCount, bufferLength, Depth, 1);
                    var depthSize = output.Step / bufferLength;
                    var buffer = new byte[bufferLength * channelCount * depthSize];
                    var bytesRead = reader.Read(buffer, 0, buffer.Length);
                    if (bytesRead == 0) observer.OnCompleted();
                    else
                    {
                        CvMat bufferHeader;
                        var bufferHandle = GCHandle.Alloc(buffer, GCHandleType.Pinned);
                        try
                        {
                            switch (Layout)
                            {
                                case MatrixLayout.ColumnMajor:
                                    bufferHeader = new CvMat(bufferLength, channelCount, Depth, 1, bufferHandle.AddrOfPinnedObject());
                                    Core.cvTranspose(bufferHeader, output);
                                    break;
                                default:
                                case MatrixLayout.RowMajor:
                                    bufferHeader = new CvMat(channelCount, bufferLength, Depth, 1, bufferHandle.AddrOfPinnedObject());
                                    Core.cvCopy(bufferHeader, output);
                                    break;
                            }
                        }
                        finally { bufferHandle.Free(); }

                        var sampleInterval = 1000.0 / Frequency;
                        var dueTime = Math.Max(0, (sampleInterval * BufferLength) - stopwatch.Elapsed.TotalMilliseconds);
                        if (dueTime > 0)
                        {
                            Thread.Sleep(TimeSpan.FromMilliseconds(dueTime));
                        }

                        observer.OnNext(output);
                    }
                }));
        }
    }
}
