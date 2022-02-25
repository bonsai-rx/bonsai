using System;
using OpenCV.Net;
using System.ComponentModel;
using Bonsai.IO;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that writes each image in the sequence to a file.
    /// </summary>
    [DefaultProperty(nameof(FileName))]
    [Description("Writes each image in the sequence to a file.")]
    public class SaveImage : Sink<IplImage>
    {
        /// <summary>
        /// Gets or sets the name of the file on which to write the images.
        /// </summary>
        [FileNameFilter("PNG Files (*.png)|*.png|BMP Files (*.bmp)|*.bmp|JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|TIFF Files (*.tif)|*.tif")]
        [Editor("Bonsai.Design.SaveFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [Description("The name of the file on which to write images.")]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the optional suffix used to generate file names.
        /// </summary>
        [Description("The optional suffix used to generate file names.")]
        public PathSuffix Suffix { get; set; }

        /// <summary>
        /// Writes each image in an observable sequence to a file.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to write.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of writing the
        /// images to the specified file.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Do(input =>
            {
                var fileName = FileName;
                if (!string.IsNullOrEmpty(fileName))
                {
                    PathHelper.EnsureDirectory(fileName);
                    fileName = PathHelper.AppendSuffix(fileName, Suffix);
                    CV.SaveImage(fileName, input);
                }
            });
        }
    }
}
