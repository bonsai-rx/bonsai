using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class GoodFeaturesToTrack : Transform<IplImage, KeyPointCollection>
    {
        public GoodFeaturesToTrack()
        {
            MaxFeatures = 100;
            QualityLevel = 0.01;
        }

        public int MaxFeatures { get; set; }

        public double QualityLevel { get; set; }

        public double MinDistance { get; set; }

        public override IObservable<KeyPointCollection> Process(IObservable<IplImage> source)
        {
            return Observable.Create<KeyPointCollection>(observer =>
            {
                IplImage temp = null;
                IplImage eigen = null;
                CvPoint2D32f[] corners = null;

                var process = source.Select(input =>
                {
                    var result = new KeyPointCollection(input);
                    temp = IplImageHelper.EnsureImageFormat(temp, input.Size, 32, 1);
                    eigen = IplImageHelper.EnsureImageFormat(eigen, input.Size, 32, 1);
                    if (corners == null || corners.Length != MaxFeatures)
                    {
                        corners = new CvPoint2D32f[MaxFeatures];
                    }

                    int cornerCount = corners.Length;
                    ImgProc.cvGoodFeaturesToTrack(input, eigen, temp, corners, ref cornerCount, QualityLevel, MinDistance, CvArr.Null, 3, 0, 0.04);
                    for (int i = 0; i < cornerCount; i++)
                    {
                        result.Add(new KeyPoint(corners[i]));
                    }

                    return result;
                }).Subscribe(observer);

                var close = Disposable.Create(() =>
                {
                    if (temp != null)
                    {
                        temp.Close();
                        eigen.Close();
                    }
                });

                return new CompositeDisposable(process, close);
            });
        }
    }
}
