using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    public class Merge : Transform<Tuple<IplImage, IplImage, IplImage, IplImage>, IplImage>
    {
        static IplImage Process(IplImage item1, IplImage item2, IplImage item3, IplImage item4)
        {
            var template = item1 ?? item2 ?? item3 ?? item4;
            if (template == null) return null;

            var channels = 0;
            if (item1 != null) channels++;
            if (item2 != null) channels++;
            if (item3 != null) channels++;
            if (item4 != null) channels++;
            var output = new IplImage(template.Size, template.Depth, channels);
            CV.Merge(item1, item2, item3, item4, output);
            return output;
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage>> source)
        {
            return source.Select(input => Process(input.Item1, input.Item2, null, null));
        }

        public IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage, IplImage>> source)
        {
            return source.Select(input => Process(input.Item1, input.Item2, input.Item3, null));
        }

        public override IObservable<IplImage> Process(IObservable<Tuple<IplImage, IplImage, IplImage, IplImage>> source)
        {
            return source.Select(input => Process(input.Item1, input.Item2, input.Item3, input.Item4));
        }
    }
}
