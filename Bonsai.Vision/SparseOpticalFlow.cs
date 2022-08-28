using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that calculates the optical flow for each sparse
    /// feature set in the sequence, using the iterative Lucas-Kanade method.
    /// </summary>
    [Description("Calculates the optical flow for each sparse feature set in the sequence, using the iterative Lucas-Kanade method.")]
    public class SparseOpticalFlow : Transform<Tuple<KeyPointCollection, IplImage>, KeyPointOpticalFlow>
    {
        /// <summary>
        /// Gets or sets the size of the search window at each pyramid level.
        /// </summary>
        [Description("The size of the search window at each pyramid level.")]
        public Size WindowSize { get; set; } = new Size(31, 31);

        /// <summary>
        /// Gets or sets the maximum pyramid level to use. If it is zero, pyramids are not used.
        /// </summary>
        [Description("The maximum pyramid level to use. If it is zero, pyramids are not used.")]
        public int Level { get; set; } = 3;

        /// <summary>
        /// Gets or sets the optional maximum allowed tracking error for each feature.
        /// </summary>
        [Description("The optional maximum allowed tracking error for each feature.")]
        public float? MaxError { get; set; }

        /// <summary>
        /// Gets or sets the maximum number of iterations.
        /// </summary>
        [Description("The maximum number of iterations.")]
        public int MaxIterations { get; set; } = 20;

        /// <summary>
        /// Gets or sets the minimum required accuracy for convergence.
        /// </summary>
        [Description("The minimum required accuracy for convergence.")]
        public double Epsilon { get; set; } = 0.03;

        /// <summary>
        /// Calculates the optical flow for each sparse feature set in an observable
        /// sequence, using the iterative Lucas-Kanade method.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="KeyPointCollection"/> objects representing the
        /// sparse feature set over which to compute the optical flow. Each element
        /// of the sequence is compared with the previous element.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="KeyPointOpticalFlow"/> objects representing the
        /// sparse correspondences between subsequent sets of features in the original
        /// sequence.
        /// </returns>
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

        /// <summary>
        /// Calculates the optical flow for each sparse feature set in an observable
        /// sequence, using the iterative Lucas-Kanade method, where each feature in
        /// the set is searched in the new image.
        /// </summary>
        /// <param name="source">
        /// A sequence of pairs where the first item is a <see cref="KeyPointCollection"/>
        /// object representing the set of features to find, and the second item is
        /// a target image on which the algorithm will try to find the features.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="KeyPointOpticalFlow"/> objects representing the
        /// sparse correspondences between each set of features in the sequence and
        /// a target image.
        /// </returns>
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
