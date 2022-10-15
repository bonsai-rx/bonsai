using System;
using System.Linq;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reactive.Linq;
using System.Globalization;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that filters the input signal using an infinite-impulse response.
    /// </summary>
    [Description("Filters the input signal using an infinite-impulse response.")]
    public class IirFilter : Transform<Mat, Mat>
    {
        static readonly double[] IdentityWeight = new[] { 1.0 };

        /// <summary>
        /// Gets or sets the feedforward filter coefficients for the infinite-impulse response.
        /// </summary>
        [XmlIgnore]
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Description("The feedforward filter coefficients for the infinite-impulse response.")]
        public double[] FeedforwardCoefficients { get; set; }

        /// <summary>
        /// Gets or sets the feedback filter coefficients for the infinite-impulse response.
        /// </summary>
        [XmlIgnore]
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Description("The feedback filter coefficients for the infinite-impulse response.")]
        public double[] FeedbackCoefficients { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the feedforward coefficients for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(FeedforwardCoefficients))]
        public string FeedforwardCoefficientsXml
        {
            get { return ArrayConvert.ToString(FeedforwardCoefficients, CultureInfo.InvariantCulture); }
            set { FeedforwardCoefficients = (double[])ArrayConvert.ToArray(value, 1, typeof(double), CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Gets or sets an XML representation of the feedback coefficients for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(FeedbackCoefficients))]
        public string FeedbackCoefficientsXml
        {
            get { return ArrayConvert.ToString(FeedbackCoefficients, CultureInfo.InvariantCulture); }
            set { FeedbackCoefficients = (double[])ArrayConvert.ToArray(value, 1, typeof(double), CultureInfo.InvariantCulture); }
        }

        void ProcessData(
            int rows,
            double[] data,
            double[] dataWeights,
            double[] dataMemory,
            double[] outputWeights,
            double[] outputMemory)
        {
            var dataLength = data.Length / rows;
            var dataMemoryLength = dataMemory.Length / rows;
            var outputMemoryLength = outputMemory.Length / rows;
            for (int i = 0; i < data.Length; i++)
            {
                var dataOutput = 0.0;
                var row = i / dataLength;
                for (int k = 0; k < dataWeights.Length; k++)
                {
                    if (k < dataMemoryLength)
                    {
                        var datum = dataMemory[row * dataMemoryLength + k];
                        dataOutput += datum * dataWeights[k];
                        if (k > 0) dataMemory[row * dataMemoryLength + k - 1] = datum;
                    }
                    else
                    {
                        dataOutput += data[i] * dataWeights[k];
                        if (dataWeights.Length > 1) dataMemory[row * dataMemoryLength + k - 1] = data[i];
                    }
                }

                for (int k = 0; k < outputWeights.Length; k++)
                {
                    if (k < outputMemoryLength)
                    {
                        var datum = outputMemory[row * outputMemoryLength + k];
                        dataOutput += datum * outputWeights[k];
                        if (k > 0) outputMemory[row * outputMemoryLength + k - 1] = datum;
                    }
                    else
                    {
                        dataOutput /= outputWeights[k];
                        if (dataWeights.Length > 1) outputMemory[row * dataMemoryLength + k - 1] = dataOutput;
                    }
                }

                data[i] = dataOutput;
            }
        }

        double[] InitializeWeights(double[] coefficients)
        {
            double[] weights;
            if (coefficients != null && coefficients.Length > 0)
            {
                weights = new double[coefficients.Length];
                for (int i = 0; i < weights.Length; i++)
                {
                    weights[i] = coefficients[weights.Length - i - 1];
                }
            }
            else weights = IdentityWeight;
            return weights;
        }

        /// <summary>
        /// Filters the input signal using the specified infinite-impulse response.
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
            return Observable.Defer(() =>
            {
                int rows = 0;
                double[] data = null;
                double[] feedforwardCoefficients = null;
                double[] feedbackCoefficients = null;
                double[] dataWeights = null;
                double[] dataMemory = null;
                double[] outputWeights = null;
                double[] outputMemory = null;
                return source.Select(input =>
                {
                    if (FeedforwardCoefficients != feedforwardCoefficients ||
                        FeedbackCoefficients != feedbackCoefficients ||
                        rows != input.Rows ||
                        data != null && data.Length != rows * input.Cols)
                    {
                        rows = input.Rows;
                        feedforwardCoefficients = FeedforwardCoefficients;
                        feedbackCoefficients = FeedbackCoefficients;
                        dataWeights = InitializeWeights(feedforwardCoefficients);
                        outputWeights = InitializeWeights(feedbackCoefficients);
                        for (int i = 0; i < outputWeights.Length - 1; i++)
                        {
                            outputWeights[i] = -outputWeights[i];
                        }

                        if (dataWeights != IdentityWeight || outputWeights != IdentityWeight)
                        {
                            data = new double[rows * input.Cols];
                            dataMemory = new double[rows * (dataWeights.Length - 1)];
                            outputMemory = new double[rows * (outputWeights.Length - 1)];
                        }
                    }

                    if (dataWeights == IdentityWeight && outputWeights == IdentityWeight)
                    {
                        return input;
                    }
                    else
                    {
                        var dataHandle = GCHandle.Alloc(data, GCHandleType.Pinned);
                        try
                        {
                            var output = new Mat(input.Size, input.Depth, input.Channels);
                            using (var dataHeader = new Mat(input.Size, Depth.F64, 1, dataHandle.AddrOfPinnedObject()))
                            {
                                CV.Convert(input, dataHeader);
                                ProcessData(rows, data, dataWeights, dataMemory, outputWeights, outputMemory);
                                CV.Convert(dataHeader, output);
                            }

                            return output;
                        }
                        finally { dataHandle.Free(); }
                    }
                });
            });
        }

        /// <summary>
        /// Filters the input signal using the specified infinite-impulse response.
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
            return Observable.Using(
                () => new Mat(1, 1, Depth.F64, 1),
                buffer =>
                {
                    return Process(source.Do(x => buffer.SetReal(0, x))
                                         .Select(x => buffer))
                                         .Select(x => x.GetReal(0));
                });
        }

        /// <summary>
        /// Filters the input position signal using the specified infinite-impulse response.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D points representing the position signal to filter.
        /// </param>
        /// <returns>
        /// A sequence of 2D points representing the filtered position signal.
        /// </returns>
        public IObservable<Point2f> Process(IObservable<Point2f> source)
        {
            return Observable.Using(
                () => new Mat(2, 1, Depth.F32, 1),
                buffer =>
                {
                    return Process(source.Do(p => { buffer.SetReal(0, 0, p.X); buffer.SetReal(1, 0, p.Y); })
                                         .Select(x => buffer))
                                         .Select(x => new Point2f(
                                             (float)x.GetReal(0, 0),
                                             (float)x.GetReal(1, 0)));
                });
        }
    }
}
