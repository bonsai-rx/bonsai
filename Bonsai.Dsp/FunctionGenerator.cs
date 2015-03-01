using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Description("Generates signal waveforms following any of a set of common periodic functions.")]
    public class FunctionGenerator : Source<Mat>
    {
        public FunctionGenerator()
        {
            PlaybackRate = 100;
            BufferLength = 441;
            Frequency = 1;
        }

        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The number of periods in each output buffer.")]
        public int Frequency { get; set; }

        [Description("The periodic waveform used to generate each output buffer.")]
        public FunctionWaveform Waveform { get; set; }

        [Description("The number of buffers generated per second.")]
        public int PlaybackRate { get; set; }

        public override IObservable<Mat> Generate()
        {
            return Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var stopwatch = new Stopwatch();
                    var buffer = new double[BufferLength];
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            stopwatch.Restart();
                            var frequency = Math.Max(1, Frequency);
                            var period = buffer.Length / frequency;
                            var indexScale = 1.0 / buffer.Length;
                            var waveform = Waveform;
                            switch (waveform)
                            {
                                default:
                                case FunctionWaveform.Sine:
                                    indexScale = 2 * Math.PI * indexScale;
                                    for (int i = 0; i < buffer.Length; i++)
                                    {
                                        buffer[i] = Math.Sin(frequency * i * indexScale);
                                    }
                                    break;
                                case FunctionWaveform.Triangular:
                                    for (int i = 0; i < buffer.Length; i++)
                                    {
                                        var t = frequency * (i + period / 4) * indexScale;
                                        buffer[i] = (1 - (4 * Math.Abs((t % 1) - 0.5) - 1)) - 1;
                                    }
                                    break;
                                case FunctionWaveform.Square:
                                case FunctionWaveform.Sawtooth:
                                    for (int i = 0; i < buffer.Length; i++)
                                    {
                                        var t = frequency * (i + period / 2) * indexScale;
                                        buffer[i] = 2 * (t % 1) - 1;
                                        if (waveform == FunctionWaveform.Square)
                                        {
                                            buffer[i] = Math.Sign(buffer[i]);
                                        }
                                    }
                                    break;
                            }

                            observer.OnNext(Mat.FromArray(buffer));
                            var playbackRate = PlaybackRate;
                            if (playbackRate <= 0)
                            {
                                throw new InvalidOperationException("Playback rate must be a positive integer.");
                            }

                            var dueTime = Math.Max(0, (1000.0 / playbackRate) - stopwatch.Elapsed.TotalMilliseconds);
                            if (dueTime > 0)
                            {
                                sampleSignal.WaitOne(TimeSpan.FromMilliseconds(dueTime));
                            }
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }
    }
}
