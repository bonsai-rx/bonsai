using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that generates a sequence of all the contours at the
    /// same level of the hierarchy, for each contour in the input sequence.
    /// </summary>
    [Description("Generates a sequence of all the contours at the same level of the hierarchy, for each contour in the input sequence.")]
    public class EnumerateContours : Combinator<Contour, Contour>
    {
        IEnumerable<Contour> GetContourEnumerator(Contour contour)
        {
            var next = contour;
            while (next != null)
            {
                yield return next;
                next = Contour.FromSeq(next.HNext);
            }
        }

        /// <summary>
        /// Generates a sequence of all the contours at the same level of the hierarchy,
        /// for each contour in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Contour"/> objects representing the first contour
        /// of the hierarchy to enumerate.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Contour"/> objects representing all the polygonal
        /// contours at the same level of the hierarchy, including each of the contours
        /// in the <paramref name="source"/> sequence.
        /// </returns>
        public override IObservable<Contour> Process(IObservable<Contour> source)
        {
            return source.SelectMany(GetContourEnumerator);
        }

        /// <summary>
        /// Generates a sequence of all the contours at the same level of the hierarchy,
        /// for each contour in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Seq"/> objects representing the first contour
        /// of the hierarchy to enumerate.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Contour"/> objects representing all the polygonal
        /// contours at the same level of the hierarchy, including each of the contours
        /// in the <paramref name="source"/> sequence.
        /// </returns>
        public IObservable<Contour> Process(IObservable<Seq> source)
        {
            return source.SelectMany(input => GetContourEnumerator(Contour.FromSeq(input)));
        }

        /// <summary>
        /// Generates a sequence of all the contours at the top level of the hierarchy,
        /// for each hierarchy of contours in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="Contours"/> objects representing the hierarchy of
        /// contours to enumerate.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Contour"/> objects representing all the polygonal
        /// contours at the top level of the hierarchy.
        /// </returns>
        public IObservable<Contour> Process(IObservable<Contours> source)
        {
            return source.SelectMany(input =>
            {
                var contour = Contour.FromSeq(input.FirstContour);
                return GetContourEnumerator(contour);
            });
        }
    }
}
