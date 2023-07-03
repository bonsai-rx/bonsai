﻿using System;
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
    /// Represents an operator that convolves the input signal with a finite-impulse
    /// response filter kernel.
    /// </summary>
    [Description("Convolves the input signal with a finite-impulse response filter kernel.")]
    public class FirFilter : Transform<Mat, Mat>
    {
        /// <summary>
        /// Gets or sets the anchor of the kernel that indicates the relative position
        /// of a filtered point within the kernel.
        /// </summary>
        [Description("The anchor of the kernel that indicates the relative position of a filtered point within the kernel.")]
        public int Anchor { get; set; } = -1;

        /// <summary>
        /// Gets or sets the convolution kernel for the FIR filter.
        /// </summary>
        [XmlIgnore]
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        [Description("The convolution kernel for the FIR filter.")]
        public float[] Kernel { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the kernel value for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Kernel))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string KernelXml
        {
            get { return ArrayConvert.ToString(Kernel, CultureInfo.InvariantCulture); }
            set { Kernel = (float[])ArrayConvert.ToArray(value, 1, typeof(float), CultureInfo.InvariantCulture); }
        }

        /// <summary>
        /// Convolves the input signal with a finite-impulse response filter kernel.
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
                Mat kernel = null;
                Mat overlap = null;
                Mat overlapInput = null;
                Mat overlapEnd = null;
                Mat overlapStart = null;
                Mat overlapFilter = null;
                Rect overlapOutput = default;
                float[] currentKernel = null;
                return source.Select(input =>
                {
                    if (Kernel != currentKernel ||
                        currentKernel != null &&
                        (input.Rows != overlapOutput.Height ||
                         input.Cols != overlapOutput.Width))
                    {
                        currentKernel = Kernel;
                        if (currentKernel == null || currentKernel.Length == 0) kernel = null;
                        else
                        {
                            kernel = new Mat(1, currentKernel.Length, Depth.F32, 1);
                            Marshal.Copy(currentKernel, 0, kernel.Data, currentKernel.Length);

                            var anchor = Anchor;
                            if (anchor == -1) anchor = kernel.Cols / 2;
                            overlap = new Mat(input.Rows, input.Cols + kernel.Cols - 1, input.Depth, input.Channels);
                            overlapInput = overlap.GetSubRect(new Rect(kernel.Cols - 1, 0, input.Cols, input.Rows));
                            overlapFilter = new Mat(overlap.Rows, overlap.Cols, overlap.Depth, overlap.Channels);
                            if (kernel.Cols > 1)
                            {
                                overlapEnd = overlap.GetSubRect(new Rect(overlap.Cols - kernel.Cols + 1, 0, kernel.Cols - 1, input.Rows));
                                overlapStart = overlap.GetSubRect(new Rect(0, 0, kernel.Cols - 1, input.Rows));
                            }

                            overlapOutput = new Rect(anchor, 0, input.Cols, input.Rows);
                            CV.CopyMakeBorder(input, overlap, new Point(kernel.Cols - 1, 0), IplBorder.Reflect);
                        }
                    }

                    if (kernel == null) return input;
                    else
                    {
                        CV.Copy(input, overlapInput);
                        CV.Filter2D(overlap, overlapFilter, kernel, new Point(Anchor, -1));
                        if (overlapEnd != null) CV.Copy(overlapEnd, overlapStart);
                        return overlapFilter.GetSubRect(overlapOutput).Clone();
                    }
                });
            });
        }

        /// <summary>
        /// Convolves the input signal with a finite-impulse response filter kernel.
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
        /// Convolves the input position signal with a finite-impulse response filter kernel.
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
