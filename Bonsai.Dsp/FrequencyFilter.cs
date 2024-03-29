﻿using System;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that filters frequencies in the input signal using a linear phase
    /// filter with the specified design parameters.
    /// </summary>
    [Description("Filters frequencies in the input signal using a linear phase filter with the specified design parameters.")]
    public class FrequencyFilter : Transform<Mat, Mat>
    {
        int kernelLength = 60;
        FilterType filterType;
        double cutoff1, cutoff2;
        int sampleRate = 44100;
        readonly FirFilter filter = new FirFilter();

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
        /// Gets or sets the size of the finite-impulse response kernel used to
        /// design the linear filter.
        /// </summary>
        [TypeConverter(typeof(KernelLengthConverter))]
        [Description("The size of the finite-impulse response kernel used to design the linear filter.")]
        public int KernelLength
        {
            get { return kernelLength; }
            set
            {
                kernelLength = value;
                UpdateFilter();
            }
        }

        /// <summary>
        /// Gets or sets a value specifying the type of filter to apply on the signal.
        /// </summary>
        [Description("Specifies the type of filter to apply on the signal.")]
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
            float[] kernel = null;
            var sampleRate = SampleRate;
            if (sampleRate > 0)
            {
                var filterType = FilterType;
                var kernelLength = KernelLength;
                if (kernelLength % 2 != 0)
                {
                    throw new InvalidOperationException("The length of the filter kernel must be an even number.");
                }

                var cutoff1 = Cutoff1;
                var cutoff2 = Cutoff2;
                kernel = CreateLowPassKernel(sampleRate, cutoff1, kernelLength);
                switch (filterType)
                {
                    case FilterType.HighPass:
                        kernel = InvertSpectrum(kernel);
                        break;
                    case FilterType.BandPass:
                    case FilterType.BandStop:
                        var highPass = InvertSpectrum(CreateLowPassKernel(sampleRate, cutoff2, kernelLength));
                        var bandStop = AddKernels(kernel, highPass);
                        if (filterType == FilterType.BandStop) kernel = bandStop;
                        else kernel = InvertSpectrum(bandStop);
                        break;
                }
            }

            filter.Kernel = kernel;
        }

        float[] CreateLowPassKernel(double sampleRate, double cutoffFrequency, int kernelLength)
        {
            // Low-pass windowed-sinc filter: http://www.dspguide.com/ch16/4.htm
            var cutoffRadians = (float)(2 * Math.PI * cutoffFrequency / sampleRate);
            var kernel = new float[kernelLength + 1];
            for (int i = 0; i < kernel.Length; i++)
            {
                var normalizer = i - kernelLength / 2f;
                if (normalizer == 0) kernel[i] = cutoffRadians;
                else kernel[i] = (float)(Math.Sin(cutoffRadians * normalizer) / normalizer);

                // Blackman window: http://www.dspguide.com/ch16/1.htm
                kernel[i] = (float)(kernel[i] * (0.42 - 0.5 * Math.Cos(2 * Math.PI * i / kernelLength)
                                                      + 0.08 * Math.Cos(4 * Math.PI * i / kernelLength)));
            }

            // Normalize for unit gain
            var sum = 0f;
            for (int i = 0; i < kernel.Length; i++)
            {
                sum += kernel[i];
            }

            for (int i = 0; i < kernel.Length; i++)
            {
                kernel[i] /= sum;
            }

            return kernel;
        }

        float[] InvertSpectrum(float[] kernel)
        {
            var inverted = new float[kernel.Length];
            for (int i = 0; i < inverted.Length; i++)
            {
                inverted[i] = -kernel[i];
            }

            inverted[(inverted.Length - 1) / 2] += 1;
            return inverted;
        }

        float[] AddKernels(float[] kernelA, float[] kernelB)
        {
            var result = new float[kernelA.Length];
            for (int i = 0; i < result.Length; i++)
            {
                result[i] = kernelA[i] + kernelB[i];
            }

            return result;
        }

        /// <summary>
        /// Filters frequencies in the input signal using a linear phase
        /// filter with the specified design parameters.
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
        /// Filters frequencies in the input signal using a linear phase
        /// filter with the specified design parameters.
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
