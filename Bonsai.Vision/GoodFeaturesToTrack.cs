using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;
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
                        result.Add(corners[i]);
                    }

                    return result;
                });
            });
        }
    }
}
