using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds the parameters of the circle with minimal
    /// area enclosing each set of 2D points in the sequence.
    /// </summary>
    [Description("Finds the parameters of the circle with minimal area enclosing each set of 2D points in the sequence.")]
    public class MinimumEnclosingCircle : Transform<Contour, Circle>
    {
        /// <summary>
        /// Finds the parameters of the circle with minimal area enclosing each array of
        /// points in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Point2f"/> arrays for which to find the minimum
        /// enclosing circle.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Circle"/> objects representing the parameters of
        /// the circle with minimal area enclosing each array of points.
        /// </returns>
        public IObservable<Circle> Process(IObservable<Point2f[]> source)
        {
            return source.Select(input =>
            {
                Circle result;
                using (var inputHeader = Mat.CreateMatHeader(input, input.Length, 2, Depth.F32, 1))
                {
                    CV.MinEnclosingCircle(inputHeader, out result.Center, out result.Radius);
                }
                return result;
            });
        }

        /// <summary>
        /// Finds the parameters of the circle with minimal area enclosing each array of
        /// points in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Mat"/> objects specifying the array of points
        /// for which to find the minimum enclosing circle.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Circle"/> objects representing the parameters of
        /// the circle with minimal area enclosing each array of points.
        /// </returns>
        public IObservable<Circle> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                Circle result;
                CV.MinEnclosingCircle(input, out result.Center, out result.Radius);
                return result;
            });
        }

        /// <summary>
        /// Finds the parameters of the circle with minimal area enclosing the array of
        /// points for each polygonal contour in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="Contour"/> objects for which to find the minimum
        /// enclosing circle.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Circle"/> objects representing the parameters of
        /// the circle with minimal area enclosing each polygonal contour.
        /// </returns>
        public override IObservable<Circle> Process(IObservable<Contour> source)
        {
            return source.Select(input =>
            {
                Circle result;
                CV.MinEnclosingCircle(input, out result.Center, out result.Radius);
                return result;
            });
        }

        /// <summary>
        /// Finds all the circles with minimal area enclosing each of the connected
        /// components in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of <see cref="ConnectedComponentCollection"/> objects
        /// representing the contours for which to find the minimum enclosing circle.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Circle"/> arrays representing the parameters of
        /// the circles with minimal area enclosing each connected component.
        /// </returns>
        public IObservable<Circle[]> Process(IObservable<ConnectedComponentCollection> source)
        {
            return source.Select(input =>
            {
                var result = new Circle[input.Count];
                for (int i = 0; i < result.Length; i++)
                {
                    CV.MinEnclosingCircle(input[i].Contour, out result[i].Center, out result[i].Radius);                    
                }
                return result;
            });
        }
    }
}
