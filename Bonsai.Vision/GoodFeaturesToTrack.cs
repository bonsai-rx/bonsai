using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds strong corner features for each image
    /// in the sequence.
    /// </summary>
    [Description("Finds strong corner features for each image in the sequence.")]
    public class GoodFeaturesToTrack : Transform<IplImage, KeyPointCollection>
    {
        /// <summary>
        /// Gets or sets the maximum number of corners to find.
        /// </summary>
        [Description("The maximum number of corners to find.")]
        public int MaxFeatures { get; set; } = 100;

        /// <summary>
        /// Gets or sets the minimal accepted quality for image corners.
        /// </summary>
        [Description("The minimal accepted quality for image corners.")]
        public double QualityLevel { get; set; } = 0.01;

        /// <summary>
        /// Gets or sets the minimum accepted distance between detected corners.
        /// </summary>
        [Description("The minimum accepted distance between detected corners.")]
        public double MinDistance { get; set; }

        /// <summary>
        /// Gets or sets the region of interest used to find image corners.
        /// If the rectangle is empty, the whole image is used.
        /// </summary>
        [Description("The region of interest used to find image corners.")]
        [Editor("Bonsai.Vision.Design.IplImageRectangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Rect RegionOfInterest { get; set; }

        /// <summary>
        /// Finds strong corner features for each image in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to find strong corner features.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="KeyPointCollection"/> objects representing the
        /// set of strong corner features extracted from each image in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<KeyPointCollection> Process(IObservable<IplImage> source)
        {
            return Process(source.Select(input => Tuple.Create(input, default(IplImage))));
        }

        /// <summary>
        /// Finds strong corner features for each image in an observable sequence,
        /// where each image is paired with a mask where zero pixels are used to indicate
        /// areas in the original image from which features should be rejected.
        /// </summary>
        /// <param name="source">
        /// A sequence of image pairs, where the first image is used to find corner
        /// features, and the second image specifies the operation mask, where zero pixels
        /// represent pixels from the original image that should be ignored.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="KeyPointCollection"/> objects representing the
        /// set of strong corner features extracted from each image in the
        /// <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<KeyPointCollection> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return Observable.Defer(() =>
            {
                IplImage temp = null;
                IplImage eigen = null;
                Point2f[] corners = null;
                return source.Select(input =>
                {
                    var image = input.Item1;
                    var mask = input.Item2;
                    var result = new KeyPointCollection(image);
                    var rect = RegionOfInterest;
                    if (rect.Width > 0 && rect.Height > 0)
                    {
                        image = image.GetSubRect(rect);
                        mask = mask != null ? mask.GetSubRect(rect) : mask;
                    }
                    else rect.X = rect.Y = 0;

                    temp = IplImageHelper.EnsureImageFormat(temp, image.Size, IplDepth.F32, 1);
                    eigen = IplImageHelper.EnsureImageFormat(eigen, image.Size, IplDepth.F32, 1);
                    if (corners == null || corners.Length != MaxFeatures)
                    {
                        corners = new Point2f[MaxFeatures];
                    }

                    int cornerCount = corners.Length;
                    CV.GoodFeaturesToTrack(image, eigen, temp, corners, out cornerCount, QualityLevel, MinDistance, mask);
                    for (int i = 0; i < cornerCount; i++)
                    {
                        corners[i].X += rect.X;
                        corners[i].Y += rect.Y;
                        result.Add(corners[i]);
                    }

                    return result;
                });
            });
        }
    }
}
