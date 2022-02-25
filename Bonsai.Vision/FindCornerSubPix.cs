using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds the sub-pixel accurate locations of
    /// each corner or radial saddle point in the sequence.
    /// </summary>
    [Description("Finds the sub-pixel accurate locations of each corner or radial saddle point in the sequence.")]
    public class FindCornerSubPix : Transform<KeyPointCollection, KeyPointCollection>
    {
        /// <summary>
        /// Gets or sets the half-length of the side of the corner search window.
        /// </summary>
        [Description("The half-length of the side of the corner search window.")]
        public Size WindowSize { get; set; } = new Size(15, 15);

        /// <summary>
        /// Gets or sets the half-length of the side of the middle search window
        /// that will be ignored during refinement.
        /// </summary>
        [Description("The half-length of the side of the middle search window that will be ignored during refinement.")]
        public Size ZeroZone { get; set; } = new Size(-1, -1);

        /// <summary>
        /// Gets or sets the maximum number of iterations.
        /// </summary>
        [Description("The maximum number of iterations.")]
        public int MaxIterations { get; set; } = 20;

        /// <summary>
        /// Gets or sets the minimum required accuracy for convergence.
        /// </summary>
        [Description("The minimum required accuracy for convergence.")]
        public double Epsilon { get; set; } = 0.01;

        /// <summary>
        /// Finds the sub-pixel accurate locations of each corner or radial saddle
        /// point in an observable sequence.
        /// </summary>
        /// <param name="source">The sequence of corner positions to refine.</param>
        /// <returns>The sequence of refined corner positions.</returns>
        public override IObservable<KeyPointCollection> Process(IObservable<KeyPointCollection> source)
        {
            return source.Select(input =>
            {
                var corners = input.ToArray();
                var maxIterations = MaxIterations;
                var epsilon = Epsilon;
                var terminationType = TermCriteriaType.None;
                if (maxIterations > 0) terminationType |= TermCriteriaType.MaxIter;
                if (epsilon > 0) terminationType |= TermCriteriaType.Epsilon;
                var termCriteria = new TermCriteria(terminationType, maxIterations, epsilon);
                CV.FindCornerSubPix(input.Image, corners, WindowSize, ZeroZone, termCriteria);

                var result = new KeyPointCollection(input.Image);
                for (int i = 0; i < corners.Length; i++)
                {
                    result.Add(corners[i]);
                }
                return result;
            });
        }
    }
}
