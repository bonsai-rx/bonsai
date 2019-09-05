using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using System.Reactive;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(TimestampedIplImageVisualizer), Target = typeof(Timestamped<IplImage>))]

namespace Bonsai.Vision.Design
{
    public class TimestampedIplImageVisualizer : IplImageVisualizer
    {
        ToolStripStatusLabel timestampLabel;

        public override void Show(object value)
        {
            var timestampedImage = (Timestamped<IplImage>)value;
            timestampLabel.Text = string.Format("Timestamp: {0}", timestampedImage.Timestamp.ToString("dd.MM.yyyy hh:mm.ss:ffff"));
            base.Show(timestampedImage.Value);
        }

        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            timestampLabel = new ToolStripStatusLabel();
            StatusStrip.Items.Add(timestampLabel);
        }

        public override void Unload()
        {
            timestampLabel = null;
            base.Unload();
        }
    }
}
