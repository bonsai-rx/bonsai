using System;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using System.Reactive;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(TimestampedIplImageVisualizer), Target = typeof(Timestamped<IplImage>))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that displays images in a sequence and adds
    /// the timestamp for each image to the status bar.
    /// </summary>
    public class TimestampedIplImageVisualizer : IplImageVisualizer
    {
        ToolStripStatusLabel timestampLabel;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var timestampedImage = (Timestamped<IplImage>)value;
            timestampLabel.Text = string.Format("Timestamp: {0}", timestampedImage.Timestamp.ToString("dd.MM.yyyy hh:mm.ss:ffff"));
            base.Show(timestampedImage.Value);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            base.Load(provider);
            timestampLabel = new ToolStripStatusLabel();
            StatusStrip.Items.Add(timestampLabel);
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            timestampLabel = null;
            base.Unload();
        }
    }
}
