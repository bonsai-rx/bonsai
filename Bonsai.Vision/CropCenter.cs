using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that crops rectangular regions with fixed size around
    /// the specified center for each image in the sequence.
    /// </summary>
    [Description("Crops rectangular regions with fixed size around the specified center for each image in the sequence.")]
    public class CropCenter : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets a value specifying the size of the region of interest to
        /// crop from the image.
        /// </summary>
        [Description("Specifies the size of the region of interest to crop from the image.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Scalar"/> specifying the value to which all
        /// pixels that fall outside image boundaries will be set to.
        /// </summary>
        [Description("Specifies the value to which all pixels that fall outside image boundaries will be set to.")]
        public Scalar FillValue { get; set; }

        static void EnsureSize(IplImage image, ref Size cropSize)
        {
            if (cropSize.Width == 0) cropSize.Width = image.Width;
            if (cropSize.Height == 0) cropSize.Height = image.Height;
        }

        static Point CenterOffset(Point centroid, Size cropSize)
        {
            centroid.X = cropSize.Width / 2 - centroid.X;
            centroid.Y = cropSize.Height / 2 - centroid.Y;
            return centroid;
        }

        static Point CenterOffset(Point2f centroid, Size cropSize)
        {
            centroid.X = cropSize.Width / 2f - centroid.X;
            centroid.Y = cropSize.Height / 2f - centroid.Y;
            return new Point(centroid);
        }

        /// <summary>
        /// Crops a rectangular region with fixed size around the center of each image
        /// in an observable sequence.
        /// </summary>
        /// <param name="source">The sequence of images to crop.</param>
        /// <returns>
        /// A sequence of images representing the cropped rectangular regions.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(image => IplImageHelper.CropMakeBorder(
                image,
                Size,
                null,
                IplBorder.Constant,
                FillValue));
        }

        /// <summary>
        /// Crops a rectangular region with fixed size around the specified center for
        /// each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs representing the image and a 2D position with integer
        /// coordinates around which to crop the rectangular region.
        /// </param>
        /// <returns>
        /// A sequence of images representing the rectangular region cropped around
        /// each of the specified positions.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point>> source)
        {
            return source.Select(value =>
            {
                var size = Size;
                var image = value.Item1;
                EnsureSize(image, ref size);
                var offset = CenterOffset(value.Item2, size);
                return IplImageHelper.CropMakeBorder(
                    image,
                    size,
                    offset,
                    IplBorder.Constant,
                    FillValue);
            });
        }

        /// <summary>
        /// Crops a rectangular region with fixed size around the specified center for
        /// each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs representing the image and a 2D position with single-precision
        /// floating-point coordinates around which to crop the rectangular region.
        /// </param>
        /// <returns>
        /// A sequence of images representing the rectangular region cropped around
        /// each of the specified positions.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, Point2f>> source)
        {
            return source.Select(value =>
            {
                var size = Size;
                var image = value.Item1;
                EnsureSize(image, ref size);
                var offset = CenterOffset(value.Item2, size);
                return IplImageHelper.CropMakeBorder(
                    image,
                    size,
                    offset,
                    IplBorder.Constant,
                    FillValue);
            });
        }

        /// <summary>
        /// Crops a rectangular region with fixed size around the center of the specified
        /// connected component for each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs representing the image and the <see cref="ConnectedComponent"/>
        /// around which to crop the rectangular region.
        /// </param>
        /// <returns>
        /// A sequence of images representing the rectangular region cropped around
        /// the centroid of the specified connected component.
        /// </returns>
        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, ConnectedComponent>> source)
        {
            return source.Select(value =>
            {
                var size = Size;
                var image = value.Item1;
                EnsureSize(image, ref size);
                var offset = CenterOffset(value.Item2.Centroid, size);
                return IplImageHelper.CropMakeBorder(
                    image,
                    size,
                    offset,
                    IplBorder.Constant,
                    FillValue);
            });
        }

        /// <summary>
        /// Crops a collection of rectangular regions with fixed size around the center of
        /// each connected component for each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs representing the image and the <see cref="ConnectedComponentCollection"/>
        /// specifying the centroids used to crop the rectangular regions.
        /// </param>
        /// <returns>
        /// A sequence of image arrays representing the rectangular regions cropped around
        /// each of the connected components.
        /// </returns>
        public IObservable<IplImage[]> Process(IObservable<Tuple<IplImage, ConnectedComponentCollection>> source)
        {
            return source.Select(value =>
            {
                var size = Size;
                var image = value.Item1;
                EnsureSize(image, ref size);
                return value.Item2.Select(component =>
                {
                    var offset = CenterOffset(component.Centroid, size);
                    return IplImageHelper.CropMakeBorder(
                        image,
                        size,
                        offset,
                        IplBorder.Constant,
                        FillValue);
                }).ToArray();
            });
        }
    }
}
