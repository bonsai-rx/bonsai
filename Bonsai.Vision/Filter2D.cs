using System;
using System.Linq;
using OpenCV.Net;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Reactive.Linq;
using System.Globalization;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that convolves each image in the sequence with
    /// the specified kernel.
    /// </summary>
    [Description("Convolves each image in the sequence with the specified kernel.")]
    public class Filter2D : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the anchor of the kernel that indicates the relative position of filtered points.
        /// </summary>
        [Description("The anchor of the kernel that indicates the relative position of filtered points.")]
        public Point Anchor { get; set; } = new Point(-1, -1);

        /// <summary>
        /// Gets or sets a 2D array specifying the image convolution kernel.
        /// </summary>
        [XmlIgnore]
        [Description("A 2D array specifying the image convolution kernel.")]
        [TypeConverter(typeof(MultidimensionalArrayConverter))]
        public float[,] Kernel { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the image convolution kernel.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Kernel))]
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

        /// <summary>
        /// Convolves each image in an observable sequence with the specified kernel.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to convolve with the specified kernel.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the result
        /// of filtering each image with the specified convolution kernel.
        /// </returns>
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

        /// <summary>
        /// Convolves each image in an observable sequence with its paired convolution
        /// kernel.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing the image and the convolution kernel,
        /// respectively, where the kernel is specified as a <see cref="Mat"/> object.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the result
        /// of filtering each image with the corresponding convolution kernel.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Mat>> source)
        {
            return source.Select(input => Filter(input.Item1, input.Item2));
        }

        /// <summary>
        /// Convolves each image in an observable sequence with its paired convolution
        /// kernel.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs containing the image and the convolution kernel,
        /// respectively, where the kernel is specified as an <see cref="IplImage"/> object.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects representing the result
        /// of filtering each image with the corresponding convolution kernel.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input => Filter(input.Item1, input.Item2.GetMat()));
        }
    }
}
