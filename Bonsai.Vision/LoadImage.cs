using System;
using System.Linq;
using OpenCV.Net;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that loads an image from the specified file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Loads an image from the specified file.")]
    public class LoadImage : Source<IplImage>
    {
        /// <summary>
        /// Gets or sets the name of the image file.
        /// </summary>
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("Image Files|*.png;*.bmp;*.jpg;*.jpeg;*.tif;*.tiff;*.exr|PNG Files|*.png|BMP Files|*.bmp|JPEG Files|*.jpg;*.jpeg|TIFF Files|*.tif;*.tiff|EXR Files|*.exr|All Files|*.*")]
        [Description("The name of the image file.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying optional conversions applied to the
        /// loaded image.
        /// </summary>
        [Description("Specifies optional conversions applied to the loaded image.")]
        public LoadImageFlags Mode { get; set; } = LoadImageFlags.Unchanged;

        IplImage CreateImage()
        {
            var fileName = FileName;
            if (string.IsNullOrEmpty(fileName))
            {
                throw new InvalidOperationException("A valid image file path was not specified.");
            }

            var image = CV.LoadImage(FileName, Mode);
            if (image == null) throw new InvalidOperationException("Failed to load an image from the specified path.");
            return image;
        }

        /// <summary>
        /// Generates an observable sequence that contains the image loaded from the
        /// specified file.
        /// </summary>
        /// <returns>
        /// A sequence containing a single <see cref="IplImage"/> object representing
        /// the image loaded from the specified file.
        /// </returns>
        public override IObservable<IplImage> Generate()
        {
            return Observable.Defer(() => Observable.Return(CreateImage()));
        }

        /// <summary>
        /// Generates an observable sequence of images loaded from the specified file,
        /// and where each image is loaded only when an observable sequence raises a
        /// notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for loading new images.
        /// </param>
        /// <returns>
        /// The sequence of <see cref="IplImage"/> objects loaded from the specified
        /// file. The most current file name is used to load the image after each
        /// notification in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<IplImage> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => CreateImage());
        }
    }
}
