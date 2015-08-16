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
using System.Globalization;

namespace Bonsai.Vision
{
    [Description("Convolves an image with the specified kernel.")]
    public class Filter2D : Transform<IplImage, IplImage>
    {
        public Filter2D()
        {
            Anchor = new Point(-1, -1);
        }

        [Description("The anchor of the kernel that indicates the relative position of filtered points.")]
        public Point Anchor { get; set; }

        [XmlIgnore]
        [Description("The image convolution kernel.")]
        [TypeConverter(typeof(MultidimensionalArrayConverter))]
        public float[,] Kernel { get; set; }

        [Browsable(false)]
        [XmlElement("Kernel")]
        public string KernelXml
        {
            get { return ArrayConvert.ToString(Kernel, CultureInfo.InvariantCulture); }
            set { Kernel = (float[,])ArrayConvert.ToArray(value, 2, typeof(float), CultureInfo.InvariantCulture); }
        }

        IplImage Filter(IplImage image, Mat kernel)
        {
            var output = new IplImage(image.Size, image.Depth, image.Channels);
            CV.Filter2D(image, output, kernel, Anchor);
            return output;
        }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                Mat kernel = null;
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
                                using (var kernelHeader = new Mat(rows, columns, Depth.F32, 1, kernelHandle.AddrOfPinnedObject()))
                                {
                                    kernel = kernelHeader.Clone();
                                }

                            }
                            finally { kernelHandle.Free(); }
                        }
                    }

                    if (kernel == null) return input;
                    else return Filter(input, kernel);
                });
            });
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Mat>> source)
        {
            return source.Select(input => Filter(input.Item1, input.Item2));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input => Filter(input.Item1, input.Item2.GetMat()));
        }
    }
}
