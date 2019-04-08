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
        const double TwoPI = 2 * Math.PI;

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
            return phase + Math.Ceiling(-phase / TwoPI) * TwoPI;
        }

        static void FrequencyPhaseShift(
            long sampleOffset,
            double timeStep,
            double newFrequency,
            ref double frequency,
            ref double phase)
        {
            newFrequency = Math.Max(0, newFrequency);
            if (frequency != newFrequency)
            {
                phase = NormalizedPhase(sampleOffset * timeStep * TwoPI * (frequency - newFrequency) + phase);
                frequency = newFrequency;
            }
        }

        Mat CreateBuffer(int bufferLength, long sampleOffset, double frequency, double phase)
        {
            var buffer = new double[bufferLength];
            if (frequency > 0)
            {
                var period = 1.0 / frequency;
                var waveform = Waveform;
                switch (waveform)
                {
                    default:
                    case FunctionWaveform.Sine:
                        frequency = frequency * TwoPI;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            buffer[i] = Math.Sin(frequency * (i + sampleOffset) + phase);
                        }
                        break;
                    case FunctionWaveform.Triangular:
                        phase = NormalizedPhase(phase) / TwoPI;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var t = frequency * (i + sampleOffset + period / 4) + phase;
                            buffer[i] = (1 - (4 * Math.Abs((t % 1) - 0.5) - 1)) - 1;
                        }
                        break;
                    case FunctionWaveform.Square:
                    case FunctionWaveform.Sawtooth:
                        phase = NormalizedPhase(phase) / TwoPI;
                        for (int i = 0; i < buffer.Length; i++)
                        {
                            var t = frequency * (i + sampleOffset + period / 2) + phase;
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
                    var bufferLength = BufferLength;
                    if (bufferLength <= 0)
                    {
                        throw new InvalidOperationException("Buffer length must be a positive integer.");
                    }

                    var sampleRate = SampleRate;
                    if (sampleRate <= 0)
                    {
                        throw new InvalidOperationException("Sample rate must be a positive integer.");
                    }

                    var i = 0L;
                    using (var sampleSignal = new ManualResetEvent(false))
                    {
                        var stopwatch = new Stopwatch();
                        stopwatch.Start();

                        var frequency = 0.0;
                        var phaseShift = 0.0;
                        var timeStep = 1.0 / sampleRate;
                        var playbackRate = sampleRate / (double)bufferLength;
                        var playbackInterval = 1000.0 / playbackRate;
                        while (!cancellationToken.IsCancellationRequested)
                        {
                            var sampleOffset = i++ * bufferLength;
                            FrequencyPhaseShift(sampleOffset, timeStep, Frequency, ref frequency, ref phaseShift);
                            var buffer = CreateBuffer(bufferLength, sampleOffset, frequency * timeStep, Phase + phaseShift);
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
                var bufferLength = BufferLength;
                if (bufferLength <= 0)
                {
                    throw new InvalidOperationException("Buffer length must be a positive integer.");
                }

                var sampleRate = SampleRate;
                if (sampleRate <= 0)
                {
                    throw new InvalidOperationException("Sample rate must be a positive integer.");
                }

                var i = 0L;
                var frequency = 0.0;
                var phaseShift = 0.0;
                var timeStep = 1.0 / sampleRate;
                return source.Select(x =>
                {
                    var sampleOffset = i++ * bufferLength;
                    FrequencyPhaseShift(sampleOffset, timeStep, Frequency, ref frequency, ref phaseShift);
                    return CreateBuffer(bufferLength, sampleOffset, frequency * timeStep, Phase + phaseShift);
                });
            });
        }
    }
}
