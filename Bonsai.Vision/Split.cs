using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    public class Split : Transform<IplImage, Tuple<IplImage, IplImage, IplImage, IplImage>>
    {
        public override IObservable<Tuple<IplImage, IplImage, IplImage, IplImage>> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var c0 = input.Channels > 1 ? new IplImage(input.Size, input.Depth, 1) : input;
                var c1 = input.Channels > 1 ? new IplImage(input.Size, input.Depth, 1) : null;
                var c2 = input.Channels > 2 ? new IplImage(input.Size, input.Depth, 1) : null;
                var c3 = input.Channels > 3 ? new IplImage(input.Size, input.Depth, 1) : null;
                if (input.Channels > 1)
                {
                    CV.Split(input, c0, c1, c2, c3);
                }

                return Tuple.Create(c0, c1, c2, c3);
            });
        }
    }
}
