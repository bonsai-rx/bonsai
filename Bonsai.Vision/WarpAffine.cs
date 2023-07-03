using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies an affine transformation to each image
    /// in the sequence.
    /// </summary>
    [Description("Applies an affine transformation to each image in the sequence.")]
    public class WarpAffine : Transform<IplImage, IplImage>
    {
        /// <summary>
        /// Gets or sets the 2x3 affine transformation matrix.
        /// </summary>
        [XmlIgnore]
        [Description("The 2x3 affine transformation matrix.")]
        [TypeConverter("Bonsai.Dsp.MatConverter, Bonsai.Dsp")]
        public Mat Transform { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the affine transformation matrix
        /// for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Transform))]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public string TransformXml
        {
            get
            {
                var transform = Transform;
                if (transform == null) return null;

                var array = new float[transform.Rows, transform.Cols];
                using (var arrayHeader = Mat.CreateMatHeader(array))
                {
                    CV.Convert(transform, arrayHeader);
                }

                return ArrayConvert.ToString(array, CultureInfo.InvariantCulture);
            }
            set
            {
                var transform = (float[,])ArrayConvert.ToArray(value, 2, typeof(float), CultureInfo.InvariantCulture);
                Transform = Mat.FromArray(transform);
            }
        }

        /// <summary>
        /// Gets or sets a value specifying the interpolation and operation flags
        /// for the image warp.
        /// </summary>
        [Description("Specifies the interpolation and operation flags for the image warp.")]
        public WarpFlags Flags { get; set; } = WarpFlags.Linear;

        /// <summary>
        /// Gets or sets the value to which all outlier pixels will be set to.
        /// </summary>
        [Description("The value to which all outlier pixels will be set to.")]
        public Scalar FillValue { get; set; }

        /// <summary>
        /// Applies an affine transformation to each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images to warp.
        /// </param>
        /// <returns>
        /// The sequence of warped images.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var mapMatrix = Transform;
                if (mapMatrix == null) return input;
                else
                {
                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    CV.WarpAffine(input, output, mapMatrix, Flags | WarpFlags.FillOutliers, FillValue);
                    return output;
                }
            });
        }
    }
}
