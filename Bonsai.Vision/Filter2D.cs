using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class Filter2D : Transform<IplImage, IplImage>
    {
        public Filter2D()
        {
            Anchor = new CvPoint(-1, -1);
        }

        public CvPoint Anchor { get; set; }

        [XmlIgnore]
        [TypeConverter(typeof(MultidimensionalArrayConverter))]
        public float[,] Kernel { get; set; }

        [Browsable(false)]
        [XmlElement("Kernel")]
        public string KernelXml
        {
            get { return ArrayConvert.ToString(Kernel); }
            set { Kernel = (float[,])ArrayConvert.ToArray(value, 2, typeof(float)); }
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                CvMat kernel = null;
                float[,] currentKernel = null;
                return source.Select(input =>
                {
                    if (Kernel != currentKernel)
                    {
                        currentKernel = Kernel;
                        if (currentKernel != null && currentKernel.Length > 0)
                        {
                            var rows = currentKernel.GetLength(0);
                            var columns = currentKernel.GetLength(1);
                            var kernelHandle = GCHandle.Alloc(currentKernel, GCHandleType.Pinned);
                            try
                            {
                                using (var kernelHeader = new CvMat(rows, columns, CvMatDepth.CV_32F, 1, kernelHandle.AddrOfPinnedObject()))
                                {
                                    kernel = kernelHeader.Clone();
                                }

                            }
                            finally { kernelHandle.Free(); }
                        }
                    }

                    if (kernel == null) return input;
                    else
                    {
                        var output = new IplImage(input.Size, input.Depth, input.NumChannels);
                        ImgProc.cvFilter2D(input, output, kernel, Anchor);
                        return output;
                    }
                });
            });
        }
    }
}
