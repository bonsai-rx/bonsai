using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Vision
{
    [Description("Applies an affine transformation to the input image.")]
    public class WarpAffine : Transform<IplImage, IplImage>
    {
        public WarpAffine()
        {
            Flags = WarpFlags.Linear;
        }

        [XmlIgnore]
        [Description("The 2x3 affine transformation matrix.")]
        [TypeConverter("Bonsai.Dsp.MatConverter, Bonsai.Dsp")]
        public Mat Transform { get; set; }

        [Browsable(false)]
        [XmlElement("Transform")]
        public string TransformXml
        {
            get
            {
                var transform = (Mat)Transform;
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

        [Description("Specifies interpolation and operation flags for the image warp.")]
        public WarpFlags Flags { get; set; }

        [Description("The value to which all outlier pixels will be set to.")]
        public Scalar FillValue { get; set; }

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
