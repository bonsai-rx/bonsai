using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Determines strong corner features in the input image.")]
    public class GoodFeaturesToTrack : Transform<IplImage, KeyPointCollection>
    {
        public GoodFeaturesToTrack()
        {
            MaxFeatures = 100;
            QualityLevel = 0.01;
        }

        [Description("The maximum number of corners to find.")]
        public int MaxFeatures { get; set; }

        [Description("The minimal accepted quality of image corners.")]
        public double QualityLevel { get; set; }

        [Description("The minimum accepted distance between detected corners.")]
        public double MinDistance { get; set; }

        [Description("The optional region of interest used to find image corners.")]
        [Editor("Bonsai.Vision.Design.IplImageInputRectangleEditor, Bonsai.Vision.Design", DesignTypes.UITypeEditor)]
        public Rect RegionOfInterest { get; set; }

        public override IObservable<KeyPointCollection> Process(IObservable<IplImage> source)
        {
            return Process(source.Select(input => Tuple.Create(input, default(IplImage))));
        }

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
