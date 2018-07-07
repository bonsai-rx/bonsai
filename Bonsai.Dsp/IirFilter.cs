using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Runtime.InteropServices;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.Globalization;

namespace Bonsai.Dsp
{
    [Description("Transforms the input signal using an infinite-impulse response filter.")]
    public class IirFilter : Transform<Mat, Mat>
    {
        static readonly double[] IdentityWeight = new[] { 1.0 };

        [XmlIgnore]
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Description("The feedforward filter coefficients for the IIR filter.")]
        public double[] FeedforwardCoefficients { get; set; }

        [XmlIgnore]
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Description("The feedback filter coefficients for the IIR filter.")]
        public double[] FeedbackCoefficients { get; set; }

        [Browsable(false)]
        [XmlElement("FeedforwardCoefficients")]
        public string FeedforwardCoefficientsXml
        {
            get { return ArrayConvert.ToString(FeedforwardCoefficients, CultureInfo.InvariantCulture); }
            set { FeedforwardCoefficients = (double[])ArrayConvert.ToArray(value, 1, typeof(double), CultureInfo.InvariantCulture); }
        }

        [Browsable(false)]
        [XmlElement("FeedbackCoefficients")]
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
