using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenTK;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading;
using System.ComponentModel.Design;
using Size = System.Drawing.Size;
using Timer = System.Windows.Forms.Timer;

[assembly: TypeVisualizer(typeof(IplImageVisualizer), Target = typeof(IplImage))]

namespace Bonsai.Vision.Design
{
    public class IplImageVisualizer : DialogMashupVisualizer
    {
        const int TargetInterval = 16;
        UserControl imagePanel;
        StatusStrip statusStrip;
        ToolStripStatusLabel statusLabel;
        VisualizerCanvas visualizerCanvas;
        IplImageTexture imageTexture;
        IplImage visualizerImage;
        IList<object> activeValues;
        IList<object> drawnValues;
        Timer updateTimer;

        protected bool StatusStripEnabled { get; set; }

        protected StatusStrip StatusStrip
        {
            get { return statusStrip; }
        }

        public IplImage VisualizerImage
        {
            get { return visualizerImage; }
        }

        public VisualizerCanvas VisualizerCanvas
        {
            get { return visualizerCanvas; }
        }

        IEnumerable<T> EnumerableMashup<T>(T first, IEnumerable<T> mashups)
        {
            yield return first;
            foreach (var mashup in mashups)
            {
                yield return mashup;
            }
        }

        protected virtual void ShowMashup(IList<object> values)
        {
            drawnValues = values;
            foreach (var mashupValue in values.Zip(EnumerableMashup(this, Mashups.Select(xs => (DialogTypeVisualizer)xs.Visualizer)), (value, visualizer) => new { value, visualizer }))
            {
                mashupValue.visualizer.Show(mashupValue.value);
            }

            visualizerCanvas.MakeCurrent();
            if (visualizerImage != null)
            {
                imageTexture.Update(visualizerImage);
            }
            visualizerCanvas.Canvas.Invalidate();
        }

        public override void Show(object value)
        {
            var inputImage = (IplImage)value;
            if (Mashups.Count > 0)
            {
                visualizerImage = IplImageHelper.EnsureImageFormat(visualizerImage, inputImage.Size, inputImage.Depth, inputImage.Channels);
                CV.Copy(inputImage, visualizerImage);
            }
            else visualizerImage = inputImage;
            UpdateStatus();
        }

        protected virtual void RenderFrame()
        {
            imageTexture.Draw();
        }

        private void UpdateStatus()
        {
            if (visualizerImage != null && statusStrip.Visible)
            {
                var cursorPosition = visualizerCanvas.Canvas.PointToClient(Form.MousePosition);
                if (visualizerCanvas.ClientRectangle.Contains(cursorPosition))
                {
                    var imageX = (int)(cursorPosition.X * ((float)visualizerImage.Width / visualizerCanvas.Width));
                    var imageY = (int)(cursorPosition.Y * ((float)visualizerImage.Height / visualizerCanvas.Height));
                    var cursorColor = visualizerImage[imageY, imageX];
                    statusLabel.Text = string.Format("Cursor: ({0},{1}) Value: ({2},{3},{4})", imageX, imageY, cursorColor.Val0, cursorColor.Val1, cursorColor.Val2);
                }
            }
        }

        public override void Load(IServiceProvider provider)
        {
            StatusStripEnabled = true;
            visualizerCanvas = new VisualizerCanvas { Dock = DockStyle.Fill };
            statusStrip = new StatusStrip { Visible = false };
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            visualizerCanvas.RenderFrame += (sender, e) => RenderFrame();
            visualizerCanvas.Load += (sender, e) => imageTexture = new IplImageTexture();
            visualizerCanvas.Canvas.MouseClick += (sender, e) => statusStrip.Visible =
                StatusStripEnabled &&
                e.Button == MouseButtons.Right ? !statusStrip.Visible : statusStrip.Visible;

            visualizerCanvas.Canvas.MouseMove += (sender, e) => UpdateStatus();
            visualizerCanvas.Canvas.MouseDoubleClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (visualizerImage != null)
                    {
                        imagePanel.Parent.ClientSize = new Size(visualizerImage.Width, visualizerImage.Height);
                    }
                }
            };

            imagePanel = new UserControl();
            imagePanel.SuspendLayout();
            imagePanel.Dock = DockStyle.Fill;
            imagePanel.Size = new Size(320, 240);
            imagePanel.AutoScaleDimensions = new SizeF(6F, 13F);
            imagePanel.AutoScaleMode = AutoScaleMode.Font;
            imagePanel.Controls.Add(visualizerCanvas);
            imagePanel.Controls.Add(statusStrip);
            imagePanel.ResumeLayout(false);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                updateTimer = new Timer();
                updateTimer.Interval = TargetInterval;
                updateTimer.Tick += updateTimer_Tick;
                visualizerService.AddControl(imagePanel);
                updateTimer.Start();
            }

            base.Load(provider);
        }

        void updateTimer_Tick(object sender, EventArgs e)
        {
            var values = Interlocked.Exchange(ref activeValues, null);
            if (values != drawnValues)
            {
                UpdateCanvas(values);
            }

            drawnValues = null;
        }

        void UpdateCanvas(IList<object> values)
        {
            var canvas = visualizerCanvas;
            if (values != null && canvas != null)
            {
                canvas.BeginInvoke((Action<IList<object>>)ShowMashup, values);
            }
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            IObservable<IList<object>> dataSource;
            var mergedSource = source.SelectMany(xs => xs.Do(
                ys => { },
                () => visualizerCanvas.BeginInvoke((Action)SequenceCompleted)));

            if (Mashups.Count > 0)
            {
                var mergedMashups = Mashups.Select(xs => xs.Visualizer.Visualize(xs.Source, provider).Publish().RefCount()).ToArray();
                dataSource = Observable
                    .CombineLatest(EnumerableMashup(mergedSource, mergedMashups))
                    .Window(mergedMashups.Last())
                    .SelectMany(window => window.TakeLast(1));
            }
            else dataSource = mergedSource.Select(xs => new[] { xs });

            return dataSource.Do(xs =>
            {
                if (Interlocked.Exchange(ref activeValues, xs) == null)
                {
                    UpdateCanvas(xs);
                }
            });
        }

        public override void Unload()
        {
            base.Unload();
            updateTimer.Stop();
            updateTimer.Dispose();
            imageTexture.Dispose();
            imagePanel.Dispose();
            updateTimer = null;
            imagePanel = null;
            statusStrip = null;
            visualizerCanvas = null;
            imageTexture = null;
            visualizerImage = null;
        }
    }
}
