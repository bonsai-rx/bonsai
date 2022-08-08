using System;
using System.Linq;
using System.Reactive.Linq;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that resizes the border around each image in the
    /// sequence without stretching the image.
    /// </summary>
    [Description("Resizes the border around each image in the sequence without stretching the image.")]
    public class ResizeCanvas : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the size of the output image.
        /// </summary>
        [Description("The size of the output image.")]
        public Size Size { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the type of border to create around
        /// the output image.
        /// </summary>
        [Description("Specifies the type of border to create around the output image.")]
        public IplBorder BorderType { get; set; } = IplBorder.Constant;

        /// <summary>
        /// Gets or sets the value to which constant border pixels will be set to.
        /// </summary>
        [Description("The value to which constant border pixels will be set to.")]
        public Scalar FillValue { get; set; }

        /// <summary>
        /// Gets or sets the optional top-left coordinates where the source image
        /// will be placed.
        /// </summary>
        [TypeConverter(typeof(NumericRecordConverter))]
        [Description("The optional top-left coordinates where the source image will be placed.")]
        public Point? Offset { get; set; }

        /// <summary>
        /// Resizes the border around each image in an observable sequence without
        /// stretching the image.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to resize the border.
        /// </param>
        /// <returns>
        /// A sequence of images with the specified border size.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(image => IplImageHelper.CropMakeBorder(
                image,
                Size,
                Offset,
                BorderType,
                FillValue));
        }
    }
}
