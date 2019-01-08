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
    [Description("For every input contour, enumerates the sequence of elements in the same level of the hierarchy.")]
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

        public override IObservable<Contour> Process(IObservable<Contour> source)
        {
            return source.SelectMany(GetContourEnumerator);
        }

        public IObservable<Contour> Process(IObservable<Seq> source)
        {
            return source.SelectMany(input => GetContourEnumerator(Contour.FromSeq(input)));
        }

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
