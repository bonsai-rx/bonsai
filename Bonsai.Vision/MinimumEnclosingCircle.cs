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
    [Description("Finds the circumscribed circle of minimal area for a given set of 2D points.")]
    public class MinimumEnclosingCircle : Transform<Contour, Circle>
    {
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

        public IObservable<Circle> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                Circle result;
                CV.MinEnclosingCircle(input, out result.Center, out result.Radius);
                return result;
            });
        }

        public override IObservable<Circle> Process(IObservable<Contour> source)
        {
            return source.Select(input =>
            {
                Circle result;
                CV.MinEnclosingCircle(input, out result.Center, out result.Radius);
                return result;
            });
        }

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
