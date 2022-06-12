using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Runtime.InteropServices;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that crops a polygonal region of interest for each
    /// image in the sequence.
    /// </summary>
    [Description("Crops a polygonal region of interest for each image in the sequence.")]
    public class CropPolygon : Transform<IplImage, IplImage>
    {
        readonly bool cropOutput = true;

        /// <summary>
        /// Initializes a new instance of the <see cref="CropPolygon"/> class.
        /// </summary>
        public CropPolygon()
            : this(true)
        {
        }

        internal CropPolygon(bool crop)
        {
            cropOutput = crop;
            MaskType = ThresholdTypes.ToZero;
        }

        /// <summary>
        /// Gets or sets the array of vertices specifying each polygonal region of interest.
        /// </summary>
        [Description("The array of vertices specifying each polygonal region of interest.")]
        [Editor("Bonsai.Vision.Design.IplImageRoiEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Point[][] Regions { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the type of mask operation to apply
        /// on the region of interest.
        /// </summary>
        [TypeConverter(typeof(ThresholdTypeConverter))]
        [Description("Specifies the type of mask operation to apply on the region of interest.")]
        public ThresholdTypes MaskType { get; set; }

        /// <summary>
        /// Gets or sets a <see cref="Scalar"/> specifying the value to which all
        /// pixels that are not in the selected region will be set to.
        /// </summary>
        [Description("Specifies the value to which all pixels that are not in the selected region will be set to.")]
        public Scalar FillValue { get; set; }

        static Rect ClipRectangle(Rect rect, Size clipSize)
        {
            var clipX = rect.X < 0 ? -rect.X : 0;
            var clipY = rect.Y < 0 ? -rect.Y : 0;
            clipX += Math.Max(0, rect.X + rect.Width - clipSize.Width);
            clipY += Math.Max(0, rect.Y + rect.Height - clipSize.Height);

            rect.X = Math.Max(0, rect.X);
            rect.Y = Math.Max(0, rect.Y);
            rect.Width = rect.Width - clipX;
            rect.Height = rect.Height - clipY;
            return rect;
        }

        /// <summary>
        /// Extracts a polygonal region of interest for each image in an observable
        /// sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to extract the polygonal region of
        /// interest.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="IplImage"/> objects where each new image
        /// contains the extracted subregion of the original image.
        /// </returns>
        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return Observable.Defer(() =>
            {
                var mask = default(IplImage);
                var boundingBox = default(Rect);
                var currentRegions = default(Point[][]);

                return source.Select(input =>
                {
                    if (Regions != currentRegions)
                    {
                        currentRegions = Regions;
                        boundingBox = default(Rect);
                        if (currentRegions != null)
                        {
                            mask = new IplImage(input.Size, IplDepth.U8, 1);
                            mask.SetZero();

                            var points = currentRegions
                                .SelectMany(region => region)
                                .SelectMany(point => new[] { point.X, point.Y })
                                .ToArray();
                            if (points.Length > 0)
                            {
                                using (var mat = new Mat(1, points.Length / 2, Depth.S32, 2))
                                {
                                    Marshal.Copy(points, 0, mat.Data, points.Length);
                                    boundingBox = CV.BoundingRect(mat);
                                    boundingBox = ClipRectangle(boundingBox, input.Size);
                                }

                                CV.FillPoly(mask, currentRegions, Scalar.All(255));
                                if (cropOutput) mask = mask.GetSubRect(boundingBox);
                            }
                        }
                        else mask = null;
                    }

                    var selectionType = MaskType;
                    if (selectionType <= ThresholdTypes.BinaryInv)
                    {
                        var size = mask != null ? mask.Size : input.Size;
                        var output = new IplImage(size, IplDepth.U8, 1);
                        switch (selectionType)
                        {
                            case ThresholdTypes.Binary:
                                if (mask == null) output.SetZero();
                                else CV.Copy(mask, output);
                                break;
                            case ThresholdTypes.BinaryInv:
                                if (mask == null) output.Set(Scalar.All(255));
                                else CV.Not(mask, output);
                                break;
                            default:
                                throw new InvalidOperationException("Selection operation is not supported.");
                        }

                        return output;
                    }

                    if (currentRegions != null && boundingBox.Width > 0 && boundingBox.Height > 0)
                    {
                        var output = new IplImage(mask.Size, input.Depth, input.Channels);
                        var inputRoi = cropOutput ? input.GetSubRect(boundingBox) : input;
                        try
                        {
                            switch (selectionType)
                            {
                                case ThresholdTypes.ToZeroInv:
                                    var fillRoi = cropOutput ? inputRoi : input;
                                    CV.Copy(fillRoi, output);
                                    output.Set(FillValue, mask);
                                    break;
                                case ThresholdTypes.ToZero:
                                    output.Set(FillValue);
                                    CV.Copy(inputRoi, output, mask);
                                    break;
                                default:
                                    throw new InvalidOperationException("Selection operation is not supported.");
                            }
                        }
                        finally
                        {
                            if (inputRoi != input) inputRoi.Close();
                        }

                        return output;
                    }

                    return input;
                });
            });
        }

        class ThresholdTypeConverter : EnumConverter
        {
            public ThresholdTypeConverter(Type type)
                : base(type)
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(
                    base.GetStandardValues(context)
                    .Cast<ThresholdTypes>()
                    .Where(type => type != ThresholdTypes.Truncate &&
                                   type != ThresholdTypes.Otsu)
                    .ToArray());
            }
        }
    }
}
