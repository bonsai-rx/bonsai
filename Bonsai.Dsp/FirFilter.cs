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

namespace Bonsai.Dsp
{
    public class FirFilter : Transform<CvMat, CvMat>
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
            get { return ArrayConvert.ToString(Kernel); }
            set { Kernel = (float[])ArrayConvert.ToArray(value, 1, typeof(float)); }
        }

        public override IObservable<CvMat> Process(IObservable<CvMat> source)
        {
            return Observable.Create<CvMat>(observer =>
            {
                CvMat kernel = null;
                CvMat overlap = null;
                CvMat overlapInput = null;
                CvMat overlapEnd = null;
                CvMat overlapStart = null;
                CvRect overlapOutput = default(CvRect);
                float[] currentKernel = null;

                Action unloadKernel = () =>
                {
                    if (kernel != null)
                    {
                        kernel.Dispose();
                        kernel = null;
                        currentKernel = null;

                        overlapInput.Close();
                        if (overlapEnd != null)
                        {
                            overlapEnd.Close();
                            overlapStart.Close();
                        }

                        overlap.Close();
                        overlap = overlapInput = null;
                        overlapEnd = overlapStart = null;
                    }
                };

                var process = source.Select(input =>
                {
                    if (Kernel != currentKernel)
                    {
                        unloadKernel();
                        currentKernel = Kernel;
                        if (currentKernel != null && currentKernel.Length > 0)
                        {
                            kernel = new CvMat(1, currentKernel.Length, CvMatDepth.CV_32F, 1);
                            Marshal.Copy(currentKernel, 0, kernel.Data, currentKernel.Length);

                            var anchor = Anchor;
                            if (anchor == -1) anchor = kernel.Cols / 2;
                            overlap = new CvMat(input.Rows, input.Cols + kernel.Cols - 1, input.Depth, input.NumChannels);
                            overlapInput = overlap.GetSubRect(new CvRect(kernel.Cols - 1, 0, input.Cols, input.Rows));
                            if (kernel.Cols > 1)
                            {
                                overlapEnd = overlap.GetSubRect(new CvRect(overlap.Cols - kernel.Cols + 1, 0, kernel.Cols - 1, input.Rows));
                                overlapStart = overlap.GetSubRect(new CvRect(0, 0, kernel.Cols - 1, input.Rows));
                            }

                            overlapOutput = new CvRect(kernel.Cols - anchor - 1, 0, input.Cols, input.Rows);
                            overlap.SetZero();
                        }
                    }

                    if (kernel == null) return input;
                    else
                    {
                        var output = new CvMat(overlap.Rows, overlap.Cols, overlap.Depth, overlap.NumChannels);
                        Core.cvCopy(input, overlapInput);
                        ImgProc.cvFilter2D(overlap, output, kernel, new CvPoint(Anchor, -1));
                        if (overlapEnd != null) Core.cvCopy(overlapEnd, overlapStart);
                        return output.GetSubRect(overlapOutput);
                    }
                }).Subscribe(observer);

                var close = Disposable.Create(unloadKernel);
                return new CompositeDisposable(process, close);
            });
        }
    }
}
