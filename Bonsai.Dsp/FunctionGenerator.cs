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
            SampleRate = 44100;
            BufferLength = 441;
            Frequency = 100;
            Amplitude = 1;
        }

        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The frequency of the signal waveform, in Hz.")]
        public double Frequency { get; set; }

        [Description("The periodic waveform used to sample the signal.")]
        public FunctionWaveform Waveform { get; set; }

        [Description("The sampling rate of the generated signal waveform, in Hz.")]
        public int SampleRate { get; set; }

        [Browsable(false)]
        public int? PlaybackRate
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    SampleRate = BufferLength * value.Value;
                    Frequency *= value.Value;
                }
            }
        }

        [Browsable(false)]
        public bool PlaybackRateSpecified
        {
            get { return PlaybackRate.HasValue; }
        }

        [TypeConverter(typeof(DepthConverter))]
        [Description("The optional target bit depth of individual buffer elements.")]
        public Depth? Depth { get; set; }

        [Browsable(false)]
        public bool DepthSpecified
        {
            get { return Depth.HasValue; }
        }

        [Description("The amplitude of the signal waveform.")]
        public double Amplitude { get; set; }

        [Description("The optional DC-offset of the signal waveform.")]
        public double Offset { get; set; }

        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, typeof(UITypeEditor))]
        [Description("The optional phase offset, in radians, of the signal waveform.")]
        public double Phase { get; set; }

        static double NormalizedPhase(double phase)
        {
            const double TwoPI = 2 * Math.PI;
            phase = phase + Math.Ceiling(-phase / TwoPI) * TwoPI;
            return phase / TwoPI;
        }

        Mat CreateBuffer(int bufferLength, long sampleOffset, double timeStep)
        {
            var buffer = new double[bufferLength];
            var frequency = Math.Max(0, Frequency);
            if (frequency > 0)
            {
                var period = 1.0 / frequency;
                var phase = Phase / frequency;
                var waveform = Waveform;
                switch (waveform)
                {
                    default:
                    case FunctionWaveform.Sine:
                        timeStep = timeStep * 2 * Math.PI;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = Math.Sin(frequency * ((i + sampleOffset) * timeStep + phase));
                        }
                        break;
                    case FunctionWaveform.Triangular:
                        phase = NormalizedPhase(phase);
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var t = frequency * ((i + sampleOffset + period / 4) * timeStep + phase);
                            buffer[i] = (1 - (4 * Math.Abs((t % 1) - 0.5) - 1)) - 1;
                        }
                        break;
                    case FunctionWaveform.Square:
                    case FunctionWaveform.Sawtooth:
                        phase = NormalizedPhase(phase);
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var t = frequency * ((i + sampleOffset + period / 2) * timeStep + phase);
                            buffer[i] = 2 * (t % 1) - 1;
                            if (waveform == FunctionWaveform.Square)
                            {
                                buffer[i] = Math.Sign(buffer[i]);
                            }
                        }
                        break;
                }
            }

            var result = new Mat(1, buffer.Length, Depth.GetValueOrDefault(OpenCV.Net.Depth.F64), 1);
            using (var bufferHeader = Mat.CreateMatHeader(buffer))
            {
                CV.ConvertScale(bufferHeader, result, Amplitude, Offset);
                return result;
            }
        }

        public override IObservable<Mat> Generate()
        {
            return Observable.Create<Mat>((observer, cancellationToken) =>
            {
                return Task.Factory.StartNew(() =>
                {
                    var i = 0L;
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var bufferLength = BufferLength;
                        var sampleRate = SampleRate;
                        var playbackRate = (double)sampleRate / bufferLength;
                        if (playbackRate <= 0)
                        {
                            throw new InvalidOperationException("Sample rate and buffer length must be positive integers.");
                        }

                        var timeStep = 1.0 / sampleRate;
                        var playbackInterval = 1000.0 / playbackRate;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var buffer = CreateBuffer(bufferLength, i++ * bufferLength, timeStep);
                            observer.OnNext(buffer);

                            var sampleInterval = (int)(playbackInterval * i - stopwatch.ElapsedMilliseconds);
                            if (sampleInterval > 0)
                            {
                                sampleSignal.WaitOne(sampleInterval);
                            }
                        }
                    }
                },
                cancellationToken,
                TaskCreationOptions.LongRunning,
                TaskScheduler.Default);
            });
        }

        public IObservable<Mat> Generate<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var i = 0L;
                var bufferLength = BufferLength;
                var timeStep = 1.0 / SampleRate;
                return source.Select(x => CreateBuffer(bufferLength, i++ * bufferLength, timeStep));
            });
        }
    }
}
