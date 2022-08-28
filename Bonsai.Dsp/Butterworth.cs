using OpenCV.Net;
using System;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that filters frequencies in the input signal using a Butterworth
    /// infinite-impulse response filter with the specified design parameters.
    /// </summary>
    [Description("Filters frequencies in the input signal using a Butterworth infinite-impulse response filter with the specified design parameters.")]
    public class Butterworth : Transform<Mat, Mat>
    {
        int filterOrder = 3;
        FilterType filterType;
        double cutoff1, cutoff2;
        int sampleRate = 44100;
        readonly IirFilter filter = new IirFilter();

        /// <summary>
        /// Gets or sets the sample rate of the input signal, in Hz.
        /// </summary>
        [Description("The sample rate of the input signal, in Hz.")]
        public int SampleRate
        {
            get { return sampleRate; }
            set
            {
                sampleRate = value;
                UpdateFilter();
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
        /// Gets or sets the first cutoff frequency, in Hz, applied to the input signal.
        /// </summary>
        [Description("The first cutoff frequency, in Hz, applied to the input signal.")]
        public double Cutoff1
        {
            get { return cutoff1; }
            set
            {
                cutoff1 = value;
                UpdateFilter();
            }
        }

        /// <summary>
        /// Gets or sets the second cutoff frequency, in Hz, applied to the input signal.
        /// </summary>
        [Description("The second cutoff frequency, in Hz, applied to the input signal.")]
        public double Cutoff2
        {
            get { return cutoff2; }
            set
            {
                cutoff2 = value;
                UpdateFilter();
            }
        }

        /// <summary>
        /// Gets or sets the order of the IIR filter.
        /// </summary>
        [Description("The order of the IIR filter.")]
        public int FilterOrder
        {
            get { return filterOrder; }
            set
            {
                filterOrder = value;
                UpdateFilter();
            }
        }

        /// <summary>
        /// Gets or sets the type of filter to apply on the signal.
        /// </summary>
        [Description("The type of filter to apply on the signal.")]
        public FilterType FilterType
        {
            get { return filterType; }
            set
            {
                filterType = value;
                UpdateFilter();
            }
        }

        void UpdateFilter()
        {
            double[] b, a;
            var fs = sampleRate;
            var cutoff1 = Cutoff1 / fs;
            var cutoff2 = Cutoff2 / fs;
            var butter = FilterDesign.ButterworthPrototype(filterOrder);
            FilterDesign.GetFilterCoefficients(butter, new[] { cutoff1, cutoff2 }, filterType, out b, out a);
            filter.FeedforwardCoefficients = b;
            filter.FeedbackCoefficients = a;
        }

        /// <summary>
        /// Filters frequencies in the input signal using a Butterworth infinite-impulse
        /// response filter with the specified design parameters.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// signal to filter.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Mat"/> objects representing the waveform of the
        /// filtered signal.
        /// </returns>
        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return filter.Process(source);
        }

        /// <summary>
        /// Filters frequencies in the input signal using a Butterworth infinite-impulse
        /// response filter with the specified design parameters.
        /// </summary>
        /// <param name="source">
        /// A sequence of floating-point numbers representing the waveform of the
        /// signal to filter.
        /// </param>
        /// <returns>
        /// A sequence of floating-point numbers representing the waveform of the
        /// filtered signal.
        /// </returns>
        public IObservable<double> Process(IObservable<double> source)
        {
            return filter.Process(source);
        }
    }
}
