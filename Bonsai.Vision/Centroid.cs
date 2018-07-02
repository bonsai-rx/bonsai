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
    [Description("Computes the centroid for a set of points, image moments, or binary contour shape.")]
    public class Centroid : Transform<Point[], Point2f>
    {
        static readonly Point2f InvalidCentroid = new Point2f(float.NaN, float.NaN);

        public override IObservable<Point2f> Process(IObservable<Point[]> source)
        {
            return source.Select(input =>
            {
                if (input.Length == 0) return InvalidCentroid;
                var sum = Point2f.Zero;
                for (int i = 0; i < input.Length; i++)
                {
                    sum.X += input[i].X;
                    sum.Y += input[i].Y;
                }

                return new Point2f(sum.X / input.Length, sum.Y / input.Length);
            });
        }

        public IObservable<Point2f> Process(IObservable<IEnumerable<Point>> source)
        {
            return source.Select(input =>
            {
                var count = 0;
                var sum = Point2f.Zero;
                foreach (var point in input)
                {
                    sum.X += point.X;
                    sum.Y += point.Y;
                    count++;
                }

                if (count == 0) return InvalidCentroid;
                else return new Point2f(sum.X / count, sum.Y / count);
            });
        }

        public IObservable<Point2f> Process(IObservable<Point2f[]> source)
        {
            return source.Select(input =>
            {
                if (input.Length == 0) return InvalidCentroid;
                var sum = Point2f.Zero;
                for (int i = 0; i < input.Length; i++)
                {
                    sum.X += input[i].X;
                    sum.Y += input[i].Y;
                }

                return new Point2f(sum.X / input.Length, sum.Y / input.Length);
            });
        }

        public IObservable<Point2f> Process(IObservable<IEnumerable<Point2f>> source)
        {
            return source.Select(input =>
            {
                var count = 0;
                var sum = Point2f.Zero;
                foreach (var point in input)
                {
                    sum.X += point.X;
                    sum.Y += point.Y;
                    count++;
                }

                if (count == 0) return InvalidCentroid;
                else return new Point2f(sum.X / count, sum.Y / count);
            });
        }

        static Point2f FromMoments(Moments moments)
        {
            if (moments.M00 > 0)
            {
                var x = moments.M10 / moments.M00;
                var y = moments.M01 / moments.M00;
                return new Point2f((float)x, (float)y);
            }
            else return InvalidCentroid;
        }

        public IObservable<Point2f> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var moments = new Moments(input);
                return FromMoments(moments);
            });
        }

        public IObservable<Point2f> Process(IObservable<Contour> source)
        {
            return source.Select(input =>
            {
                var moments = new Moments(input);
                return FromMoments(moments);
            });
        }

        public IObservable<Point2f> Process(IObservable<ConnectedComponent> source)
        {
            return source.Select(input => input.Centroid);
        }
    }
}
