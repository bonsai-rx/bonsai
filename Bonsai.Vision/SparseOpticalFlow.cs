using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Calculates the optical flow for a sparse feature set using the iterative Lucas-Kanade method.")]
    public class SparseOpticalFlow : Transform<Tuple<KeyPointCollection, IplImage>, KeyPointOpticalFlow>
    {
        public SparseOpticalFlow()
        {
            Level = 3;
            MaxIterations = 20;
            Epsilon = 0.03;
            WindowSize = new Size(31, 31);
        }

        [Description("The size of the search window at each pyramid level.")]
        public Size WindowSize { get; set; }

        [Description("The maximum pyramid level to use. If it is zero, pyramids are not used.")]
        public int Level { get; set; }

        [Description("The optional maximum allowed tracking error for each feature.")]
        public float? MaxError { get; set; }

        [Description("The maximum number of iterations.")]
        public int MaxIterations { get; set; }

        [Description("The minimum required accuracy for convergence.")]
        public double Epsilon { get; set; }

        public IObservable<KeyPointOpticalFlow> Process(IObservable<KeyPointCollection> source)
        {
            return source.Publish(ps =>
            {
                var emptyPoints = Observable.Return(new KeyPointCollection(null));
                var pairings = emptyPoints.Concat(ps).Zip(ps,
                    (previous, current) => Tuple.Create(previous, current.Image));
                return Process(pairings);
            });
        }

        public override IObservable<KeyPointOpticalFlow> Process(IObservable<Tuple<KeyPointCollection, IplImage>> source)
        {
            return Observable.Defer(() =>
            {
                IplImage previousImage = null;
                IplImage previousPyramid = null;
                IplImage currentPyramid = null;
                return source.Select(input =>
                {
                    var previous = input.Item1;
                    var currentImage = input.Item2;
                    var currentKeyPoints = new KeyPointCollection(currentImage);
                    if (previous.Count == 0) return new KeyPointOpticalFlow(previous, currentKeyPoints);

                    if (currentPyramid == null || currentPyramid.Size != currentImage.Size)
                    {
                        previousImage = null;
                        previousPyramid = new IplImage(currentImage.Size, currentImage.Depth, currentImage.Channels);
                        currentPyramid = new IplImage(currentImage.Size, currentImage.Depth, currentImage.Channels);
                    }

                    var maxIterations = MaxIterations;
                    var epsilon = Epsilon;
                    var terminationType = TermCriteriaType.None;
                    if (maxIterations > 0) terminationType |= TermCriteriaType.MaxIter;
                    if (epsilon > 0) terminationType |= TermCriteriaType.Epsilon;
                    var termCriteria = new TermCriteria(terminationType, maxIterations, epsilon);
                    var flags = previousImage == previous.Image ? LKFlowFlags.PyrAReady : LKFlowFlags.None;

                    var previousFeatures = new Point2f[previous.Count];
                    for (int i = 0; i < previousFeatures.Length; i++)
                    {
                        previousFeatures[i] = previous[i];
                    }

                    var currentFeatures = new Point2f[previousFeatures.Length];
                    var status = new byte[previousFeatures.Length];
                    var trackError = new float[previousFeatures.Length];
                    CV.CalcOpticalFlowPyrLK(
                        previous.Image,
                        currentImage,
                        previousPyramid,
                        currentPyramid,
                        previousFeatures,
                        currentFeatures,
                        WindowSize,
                        Level,
                        status,
                        trackError,
                        termCriteria,
                        flags);

                    var previousKeyPoints = new KeyPointCollection(previous.Image);
                    for (int i = 0; i < status.Length; i++)
                    {
                        if (status[i] == 0 ||
                            trackError[i] > MaxError ||
                            currentFeatures[i].X < 0 ||
                            currentFeatures[i].Y < 0 ||
                            currentFeatures[i].X > currentImage.Width - 1 ||
                            currentFeatures[i].Y > currentImage.Height - 1)
                        {
                            continue;
                        }

                        previousKeyPoints.Add(previousFeatures[i]);
                        currentKeyPoints.Add(currentFeatures[i]);
                    }

                    var temp = currentPyramid;
                    currentPyramid = previousPyramid;
                    previousPyramid = temp;
                    previousImage = currentImage;
                    return new KeyPointOpticalFlow(previousKeyPoints, currentKeyPoints);
                });
            });
        }
    }
}
