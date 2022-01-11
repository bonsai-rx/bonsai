using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that generates signal waveforms following any of a
    /// set of common periodic functions.
    /// </summary>
    [Description("Generates signal waveforms following any of a set of common periodic functions.")]
    public class FunctionGenerator : Source<Mat>
    {
        const double TwoPI = 2 * Math.PI;

        /// <summary>
        /// Gets or sets the number of samples in each output buffer.
        /// </summary>
        [Description("The number of samples in each output buffer.")]
        public int BufferLength { get; set; } = 441;

        /// <summary>
        /// Gets or sets the frequency of the signal waveform, in Hz.
        /// </summary>
        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The frequency of the signal waveform, in Hz.")]
        public double Frequency { get; set; } = 100;

        /// <summary>
        /// Gets or sets a value specifying the periodic waveform used to sample the signal.
        /// </summary>
        [Description("Specifies the periodic waveform used to sample the signal.")]
        public FunctionWaveform Waveform { get; set; }

        /// <summary>
        /// Gets or sets the sampling rate of the generated signal waveform, in Hz.
        /// </summary>
        [Description("The sampling rate of the generated signal waveform, in Hz.")]
        public int SampleRate { get; set; } = 44100;

        /// <summary>
        /// Gets or sets the sampling rate of the generated signal waveform, in Hz.
        /// </summary>
        [Browsable(false)]
        [Obsolete("Use SampleRate instead for consistent wording with signal processing operator properties.")]
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

        /// <summary>
        /// Gets a value indicating whether the <see cref="PlaybackRate"/> property should be serialized.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public bool PlaybackRateSpecified
        {
            get { return PlaybackRate.HasValue; }
        }

        /// <summary>
        /// Gets or sets the bit depth of each element in an output buffer.
        /// </summary>
        /// <remarks>
        /// If this property is not specified, the bit depth of output buffers
        /// will be <see cref="Depth.F64"/>.
        /// </remarks>
        [TypeConverter(typeof(DepthConverter))]
        [Description("The optional bit depth of each element in an output buffer.")]
        public Depth? Depth { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Depth"/> property should be serialized.
        /// </summary>
        [Browsable(false)]
        public bool DepthSpecified
        {
            get { return Depth.HasValue; }
        }

        /// <summary>
        /// Gets or sets the amplitude of the signal waveform.
        /// </summary>
        [Description("The amplitude of the signal waveform.")]
        public double Amplitude { get; set; } = 1;

        /// <summary>
        /// Gets or sets the optional DC-offset of the signal waveform.
        /// </summary>
        [Description("The optional DC-offset of the signal waveform.")]
        public double Offset { get; set; }

        /// <summary>
        /// Gets or sets the optional phase offset, in radians, of the signal waveform.
        /// </summary>
        [Range(-Math.PI, Math.PI)]
        [Editor(DesignTypes.SliderEditor, DesignTypes.UITypeEditor)]
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

        /// <summary>
        /// Generates an observable sequence of buffers sampled from a signal waveform
        /// following the specified periodic function.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing signal sampling
        /// buffers of a fixed length. See <see cref="BufferLength"/>.
        /// </returns>
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

        /// <summary>
        /// Generates an observable sequence of buffers sampled from a signal waveform
        /// following the specified periodic function, and where each new buffer is
        /// emitted only when an observable sequence raises a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting signal buffers.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing signal sampling
        /// buffers of a fixed length. See <see cref="BufferLength"/>.
        /// </returns>
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
