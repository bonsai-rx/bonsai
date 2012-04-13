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

[assembly: TypeVisualizer(typeof(TimeIntervalIplImageVisualizer), Target = typeof(TimeInterval<IplImage>))]

namespace Bonsai.Vision.Design
{
    public class TimeIntervalIplImageVisualizer : IplImageVisualizer
    {
        ToolStripStatusLabel timeIntervalLabel;
        ToolStripStatusLabel fpsLabel;

        public override void Show(object value)
        {
            var timeIntervalImage = (TimeInterval<IplImage>)value;
            var interval = timeIntervalImage.Interval.TotalSeconds;
            timeIntervalLabel.Text = string.Format("Interval: {0:f4}", interval);
            fpsLabel.Text = string.Format("FPS: {0:f2}", 1 / interval);
            base.Show(timeIntervalImage.Value);
        }

        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            timeIntervalLabel = new ToolStripStatusLabel();
            fpsLabel = new ToolStripStatusLabel();
            StatusStrip.Items.Add(timeIntervalLabel);
            StatusStrip.Items.Add(fpsLabel);
        }

        public override void Unload()
        {
            timeIntervalLabel = null;
            fpsLabel = null;
            base.Unload();
        }
    }
}
