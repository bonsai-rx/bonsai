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
    public class FirFilter : Transform<Mat, Mat>
    {
        public FirFilter()
        {
            Anchor = -1;
        }

        public int Anchor { get; set; }

        [XmlIgnore]
        [TypeConverter(typeof(UnidimensionalArrayConverter))]
        public float[] Kernel { get; set; }

        [Browsable(false)]
        [XmlElement("Kernel")]
        public string KernelXml
        {
            get { return ArrayConvert.ToString(Kernel, CultureInfo.InvariantCulture); }
            set { Kernel = (float[])ArrayConvert.ToArray(value, 1, typeof(float), CultureInfo.InvariantCulture); }
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return Observable.Defer(() =>
            {
                Mat kernel = null;
                Mat overlap = null;
                Mat overlapInput = null;
                Mat overlapEnd = null;
                Mat overlapStart = null;
                Rect overlapOutput = default(Rect);
                float[] currentKernel = null;
                return source.Select(input =>
                {
                    if (Kernel != currentKernel)
                    {
                        currentKernel = Kernel;
                        if (currentKernel != null && currentKernel.Length > 0)
                        {
                            kernel = new Mat(1, currentKernel.Length, Depth.F32, 1);
                            Marshal.Copy(currentKernel, 0, kernel.Data, currentKernel.Length);

                            var anchor = Anchor;
                            if (anchor == -1) anchor = kernel.Cols / 2;
                            overlap = new Mat(input.Rows, input.Cols + kernel.Cols - 1, input.Depth, input.Channels);
                            overlapInput = overlap.GetSubRect(new Rect(kernel.Cols - 1, 0, input.Cols, input.Rows));
                            if (kernel.Cols > 1)
                            {
                                overlapEnd = overlap.GetSubRect(new Rect(overlap.Cols - kernel.Cols + 1, 0, kernel.Cols - 1, input.Rows));
                                overlapStart = overlap.GetSubRect(new Rect(0, 0, kernel.Cols - 1, input.Rows));
                            }

                            overlapOutput = new Rect(kernel.Cols - anchor - 1, 0, input.Cols, input.Rows);
                            overlap.SetZero();
                        }
                    }

                    if (kernel == null) return input;
                    else
                    {
                        var output = new Mat(overlap.Rows, overlap.Cols, overlap.Depth, overlap.Channels);
                        CV.Copy(input, overlapInput);
                        CV.Filter2D(overlap, output, kernel, new Point(Anchor, -1));
                        if (overlapEnd != null) CV.Copy(overlapEnd, overlapStart);
                        return output.GetSubRect(overlapOutput);
                    }
                });
            });
        }
    }
}
