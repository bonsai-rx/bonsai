using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using System.Reactive;

[assembly: TypeVisualizer(typeof(TimestampedIplImageVisualizer), Target = typeof(Timestamped<IplImage>))]

namespace Bonsai.Vision.Design
{
    public class TimestampedIplImageVisualizer : IplImageVisualizer
    {
        CvFont font;

        public override void Show(object value)
        {
            var timestampedImage = (Timestamped<IplImage>)value;
            var image = timestampedImage.Value.Clone();
            Core.cvPutText(image, timestampedImage.Timestamp.ToString("dd.MM.yyyy hh:mm.ss:ffff"), new CvPoint(10, 20), font, CvScalar.Rgb(255, 0, 0));
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
