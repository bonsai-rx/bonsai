using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Reactive.Linq;
using System.Threading;
using System.ComponentModel.Design;

[assembly: TypeVisualizer(typeof(IplImageVisualizer), Target = typeof(IplImage))]
[assembly: TypeVisualizer(typeof(IplImageVisualizer), Target = typeof(IObservable<IplImage>))]

namespace Bonsai.Vision.Design
{
    public class IplImageVisualizer : DialogMashupVisualizer
    {
        bool allowUpdate;
        bool canvasInvalidated;
        Panel imagePanel;
        StatusStrip statusStrip;
        ToolStripStatusLabel statusLabel;
        VisualizerCanvasControl visualizerCanvas;
        IplImageTexture imageTexture;
        IplImage visualizerImage;

        protected bool StatusStripEnabled { get; set; }

        protected StatusStrip StatusStrip
        {
            get { return statusStrip; }
        }

        public IplImage VisualizerImage
        {
            get { return visualizerImage; }
        }

        public VisualizerCanvasControl VisualizerCanvas
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
            foreach (var mashupValue in values.Zip(EnumerableMashup(this, Mashups.Select(xs => (DialogTypeVisualizer)xs.Visualizer)), (value, visualizer) => new { value, visualizer }))
            {
                mashupValue.visualizer.Show(mashupValue.value);
            }

            visualizerCanvas.MakeCurrent();
            if (visualizerImage != null) imageTexture.Update(visualizerImage);
            visualizerCanvas.Canvas.Invalidate();
            canvasInvalidated = true;
        }

        public override void Show(object value)
        {
            var inputImage = (IplImage)value;
            visualizerImage = IplImageHelper.EnsureImageFormat(visualizerImage, inputImage.Size, inputImage.Depth, inputImage.NumChannels);
            Core.cvCopy(inputImage, visualizerImage);
        }

        protected virtual void RenderFrame()
        {
            imageTexture.Draw();
        }

        private void SwapBuffers()
        {
            if (canvasInvalidated)
            {
                canvasInvalidated = false;
                allowUpdate = true;
            }
        }

        public override void Load(IServiceProvider provider)
        {
            allowUpdate = true;
            StatusStripEnabled = true;
            visualizerCanvas = new VisualizerCanvasControl { Dock = DockStyle.Fill };
            statusStrip = new StatusStrip { Visible = false };
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            visualizerCanvas.RenderFrame += (sender, e) => RenderFrame();
            visualizerCanvas.SwapBuffers += (sender, e) => SwapBuffers();
            visualizerCanvas.Load += (sender, e) => imageTexture = new IplImageTexture();
            visualizerCanvas.Canvas.MouseClick += (sender, e) => statusStrip.Visible =
                StatusStripEnabled &&
                e.Button == MouseButtons.Right ? !statusStrip.Visible : statusStrip.Visible;

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

            visualizerCanvas.Canvas.MouseMove += (sender, e) =>
            {
                if (visualizerImage != null && statusStrip.Visible)
                {
                    var cursorPosition = visualizerCanvas.Canvas.PointToClient(Form.MousePosition);
                    if (visualizerCanvas.ClientRectangle.Contains(cursorPosition))
                    {
                        var imageX = (int)(cursorPosition.X * ((float)visualizerImage.Width / visualizerCanvas.Width));
                        var imageY = (int)(cursorPosition.Y * ((float)visualizerImage.Height / visualizerCanvas.Height));
                        var cursorColor = Core.cvGet2D(visualizerImage, imageY, imageX);
                        statusLabel.Text = string.Format("Cursor: ({0},{1}) Value: ({2},{3},{4})", imageX, imageY, cursorColor.Val0, cursorColor.Val1, cursorColor.Val2);
                    }
                }
            };

            imagePanel = new Panel { Dock = DockStyle.Fill, Size = new Size(320, 240) };
            imagePanel.Controls.Add(visualizerCanvas);
            imagePanel.Controls.Add(statusStrip);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(imagePanel);
            }

            base.Load(provider);
        }

        protected IObservable<object> Visualize<T>(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            canvasInvalidated = true;
            IObservable<object> mergedSource;
            IObservable<IList<object>> dataSource;
            var visualizerContext = (ITypeVisualizerContext)provider.GetService(typeof(ITypeVisualizerContext));
            if (visualizerContext != null && typeof(IObservable<T>).IsAssignableFrom(visualizerContext.Source.ObservableType))
            {
                mergedSource = source.SelectMany(xs => xs.Select(ws => ws as IObservable<T>)
                                                         .Where(ws => ws != null)
                                                         .SelectMany(ws => ws.Select(vs => (object)vs).Do(ys => { }, () => visualizerCanvas.BeginInvoke((Action)SequenceCompleted))));
            }
            else mergedSource = source.SelectMany(xs => xs.Do(ys => { }, () => visualizerCanvas.BeginInvoke((Action)SequenceCompleted)));

            if (Mashups.Count > 0)
            {
                var mergedMashups = Mashups.Select(xs => xs.Visualizer.Visualize(xs.Source, provider).Publish().RefCount()).ToArray();
                dataSource = Observable
                    .CombineLatest(EnumerableMashup(mergedSource, mergedMashups))
                    .Window(mergedMashups.Last())
                    .SelectMany(window => window.TakeLast(1));
            }
            else dataSource = mergedSource.Select(xs => new[] { xs });

            return dataSource
                .Where(xs => allowUpdate)
                .Do(xs => allowUpdate = false)
                .ObserveOn(visualizerCanvas.Canvas)
                .Do(ShowMashup);
        }

        public override IObservable<object> Visualize(IObservable<IObservable<object>> source, IServiceProvider provider)
        {
            return Visualize<IplImage>(source, provider);
        }

        public override void Unload()
        {
            base.Unload();
            imageTexture.Dispose();
            imagePanel.Dispose();
            imagePanel = null;
            statusStrip = null;
            visualizerCanvas = null;
            imageTexture = null;
            visualizerImage = null;
        }
    }
}
