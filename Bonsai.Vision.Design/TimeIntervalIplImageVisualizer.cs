using System;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System.Reactive;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(TimeIntervalIplImageVisualizer), Target = typeof(TimeInterval<IplImage>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that displays images in a sequence and adds
    /// the interval between each image in the sequence to the status bar.
    /// </summary>
    public class TimeIntervalIplImageVisualizer : IplImageVisualizer
    {
        ToolStripStatusLabel timeIntervalLabel;
        ToolStripStatusLabel fpsLabel;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var timeIntervalImage = (TimeInterval<IplImage>)value;
            var interval = timeIntervalImage.Interval.TotalSeconds;
            timeIntervalLabel.Text = string.Format("Interval: {0:f4}", interval);
            fpsLabel.Text = string.Format("FPS: {0:f2}", 1 / interval);
            base.Show(timeIntervalImage.Value);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            timeIntervalLabel = new ToolStripStatusLabel();
            fpsLabel = new ToolStripStatusLabel();
            StatusStrip.Items.Add(timeIntervalLabel);
            StatusStrip.Items.Add(fpsLabel);
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            timeIntervalLabel = null;
            fpsLabel = null;
            base.Unload();
        }
    }
}
