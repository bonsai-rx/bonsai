using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that decreases the sampling rate of the input signal
    /// by the specified factor.
    /// </summary>
    [Description("Decreases the sampling rate of the input signal by the specified factor.")]
    public class Decimate : Combinator<Mat, Mat>
    {
        int factor;
        readonly FrequencyFilter filter = new FrequencyFilter();

        /// <summary>
        /// Initializes a new instance of the <see cref="Decimate"/> class.
        /// </summary>
        public Decimate()
        {
            Factor = 1;
            SampleRate = 44100;
            Downsampling = DownsamplingMethod.LowPass;
        }

        /// <summary>
        /// Gets or sets a value specifying the downsampling method used to decimate the input signal.
        /// </summary>
        [Description("Specifies the downsampling method used to decimate the input signal.")]
        public DownsamplingMethod Downsampling { get; set; }

        /// <summary>
        /// Gets or sets the length of each output array. If set to zero, the length
        /// of each input buffer will be used.
        /// </summary>
        [Description("The optional length of each output array. If set to zero, the length of each input buffer will be used.")]
        public int BufferLength { get; set; }

        /// <summary>
        /// Gets or sets the integral factor by which to divide the sampling rate of the input signal.
        /// </summary>
        [Description("The integral factor by which to divide the sampling rate of the input signal.")]
        public int Factor
        {
            get { return factor; }
            set
            {
                factor = value;
                UpdateCutoffFrequency(SampleRate, value);
            }
        }

        /// <summary>
        /// Gets or sets the sample rate of the input signal, in Hz.
        /// </summary>
        [Description("The sample rate of the input signal, in Hz.")]
        public int SampleRate
        {
            get { return filter.SampleRate; }
            set
            {
                filter.SampleRate = value;
                UpdateCutoffFrequency(value, Factor);
            }
        }

        /// <summary>
        /// Gets or sets the sample rate of the input signal, in Hz.
        /// </summary>
        [Browsable(false)]
        [Obsolete("Use SampleRate instead for consistent wording with signal processing operator properties.")]
        public double? SamplingFrequency
        {
            get { return null; }
            set
            {
                if (value != null)
                {
                    SampleRate = (int)value.Value;
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether the <see cref="SamplingFrequency"/> property should be serialized.
        /// </summary>
        [Obsolete]
        [Browsable(false)]
        public bool SamplingFrequencySpecified
        {
            get { return SamplingFrequency.HasValue; }
        }

        /// <summary>
        /// Gets or sets the size of the finite-impulse response kernel used to
        /// design the downsampling filter.
        /// </summary>
        [TypeConverter(typeof(KernelLengthConverter))]
        [Description("The size of the finite-impulse response kernel used to design the downsampling filter.")]
        public int KernelLength
        {
            get { return filter.KernelLength; }
            set { filter.KernelLength = value; }
        }

        void UpdateCutoffFrequency(double sampleRate, int factor)
        {
            if (factor > 0)
            {
                filter.Cutoff1 = sampleRate / (2 * factor);
            }
            else filter.Cutoff1 = 0;
        }

        static Mat CreateBuffer(int cols, Mat input)
        {
            return new Mat(input.Rows, cols, input.Depth, input.Channels);
        }

        /// <summary>
        /// Decreases the sampling rate of the input signal by the specified factor.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// signal to downsample.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// downsampled signal.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Create<Mat>(observer =>
            {
                var carry = 0;
                var index = 0;
                var offset = 0;
                var lottery = 0;
                var scaleFactor = 0.0;
                var currentFactor = 0;
                var buffer = default(Mat);
                var carryBuffer = default(Mat);
                var downsampling = Downsampling;
                var random = downsampling == DownsamplingMethod.Dithering ? new Random() : null;
                var reduceOp = (ReduceOperation)(downsampling - DownsamplingMethod.Sum);
                if (reduceOp == ReduceOperation.Avg) reduceOp = ReduceOperation.Sum;
                var downsample = downsampling == DownsamplingMethod.LowPass ? filter.Process(source) : source;
                return downsample.Subscribe(input =>
                {
                    try
                    {
                        var bufferLength = BufferLength;
                        if (bufferLength == 0) bufferLength = input.Cols;
                        if (buffer == null || buffer.Rows != input.Rows || currentFactor != factor)
                        {
                            index = 0;
                            currentFactor = factor;
                            if (downsampling >= DownsamplingMethod.Sum)
                            {
                                carry = currentFactor;
                                carryBuffer = new Mat(input.Rows, 1, input.Depth, input.Channels);
                                if (downsampling == DownsamplingMethod.Avg) scaleFactor = 1.0 / currentFactor;
                                else scaleFactor = 0;
                            }
                            else if (random != null)
                            {
                                lottery = random.Next(currentFactor);
                                offset = lottery;
                            }
                            else offset = 0;
                            buffer = CreateBuffer(bufferLength, input);
                        }

                        while (offset < input.Cols)
                        {
                            // Process decimation data on this buffer
                            Rect outputRect;
                            if (downsampling > DownsamplingMethod.LowPass)
                            {
                                outputRect = new Rect(index, 0, 1, input.Rows);
                            }
                            else
                            {
                                var samples = input.Cols - offset;
                                var whole = samples / currentFactor;
                                outputRect = new Rect(index, 0, Math.Min(buffer.Cols - index, whole), input.Rows);
                            }

                            if (downsampling >= DownsamplingMethod.Sum)
                            {
                                // Reduce decimate
                                var inputSamples = Math.Min(input.Cols - offset, carry);
                                var inputRect = new Rect(offset, 0, inputSamples, input.Rows);
                                using (var inputBuffer = input.GetSubRect(inputRect))
                                using (var outputBuffer = buffer.GetCol(index))
                                {
                                    if (carry < currentFactor)
                                    {
                                        CV.Reduce(inputBuffer, carryBuffer, 1, reduceOp);
                                        switch (reduceOp)
                                        {
                                            case ReduceOperation.Sum:
                                                CV.Add(outputBuffer, carryBuffer, outputBuffer);
                                                break;
                                            case ReduceOperation.Max:
                                                CV.Max(outputBuffer, carryBuffer, outputBuffer);
                                                break;
                                            case ReduceOperation.Min:
                                                CV.Min(outputBuffer, carryBuffer, outputBuffer);
                                                break;
                                        }
                                    }
                                    else CV.Reduce(inputBuffer, outputBuffer, 1, reduceOp);

                                    offset += inputRect.Width;
                                    carry -= inputSamples;
                                    if (carry <= 0)
                                    {
                                        index++;
                                        carry = currentFactor;
                                        if (scaleFactor > 0)
                                        {
                                            CV.ConvertScale(outputBuffer, outputBuffer, scaleFactor);
                                        }
                                    }
                                }
                            }
                            else if (outputRect.Width > 1)
                            {
                                // Block decimate
                                var inputRect = new Rect(offset, 0, outputRect.Width * currentFactor, input.Rows);
                                using (var inputBuffer = input.GetSubRect(inputRect))
                                using (var outputBuffer = buffer.GetSubRect(outputRect))
                                {
                                    CV.Resize(inputBuffer, outputBuffer, SubPixelInterpolation.NearestNeighbor);
                                }

                                index += outputRect.Width;
                                offset += inputRect.Width;
                            }
                            else
                            {
                                // Decimate single time point
                                using (var inputBuffer = input.GetCol(offset))
                                using (var outputBuffer = buffer.GetCol(index))
                                {
                                    CV.Copy(inputBuffer, outputBuffer);
                                }

                                index++;
                                if (random != null)
                                {
                                    offset += currentFactor - lottery;
                                    lottery = random.Next(currentFactor);
                                    offset += lottery;
                                }
                                else offset += currentFactor;
                            }

                            if (index >= buffer.Cols)
                            {
                                index = 0;
                                observer.OnNext(buffer);
                                buffer = CreateBuffer(bufferLength, input);
                            }
                        }

                        offset -= input.Cols;
                    }
                    catch (Exception ex)
                    {
                        observer.OnError(ex);
                    }
                },
                observer.OnError,
                () =>
                {
                    // Emit pending buffer
                    if (index > 0)
                    {
                        observer.OnNext(buffer.GetCols(0, index));
                    }

                    buffer = null;
                    observer.OnCompleted();
                });
            });
        }
    }
}
