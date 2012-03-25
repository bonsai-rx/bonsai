using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using System.Reactive;

[assembly: TypeVisualizer(typeof(TimeIntervalIplImageVisualizer), Target = typeof(TimeInterval<IplImage>))]

namespace Bonsai.Vision.Design
{
    public class TimeIntervalIplImageVisualizer : IplImageVisualizer
    {
        CvFont font;

        public override void Show(object value)
        {
            var timeIntervalImage = (TimeInterval<IplImage>)value;
            var image = timeIntervalImage.Value.Clone();
            var interval = timeIntervalImage.Interval.TotalSeconds;
            Core.cvPutText(image, string.Format("Interval: {0:f4}", interval), new CvPoint(10, 20), font, CvScalar.Rgb(255, 0, 0));
            Core.cvPutText(image, string.Format("FPS: {0:f2}", 1 / interval), new CvPoint(10, 40), font, CvScalar.Rgb(255, 0, 0));
            base.Show(image);
        }

        public override void Load(IServiceProvider provider)
        {
            font = new CvFont(1);
            base.Load(provider);
        }

        public override void Unload()
        {
            font.Close();
            base.Unload();
        }
    }
}
