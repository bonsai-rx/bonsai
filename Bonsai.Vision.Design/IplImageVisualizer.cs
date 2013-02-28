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

[assembly: TypeVisualizer(typeof(IplImageVisualizer), Target = typeof(IplImage))]

namespace Bonsai.Vision.Design
{
    public class IplImageVisualizer : DialogTypeVisualizer
    {
        bool allowUpdate;
        bool canvasInvalidated;
        IplImage image;
        Panel imagePanel;
        StatusStrip statusStrip;
        ToolStripStatusLabel statusLabel;
        VisualizerCanvasControl imageControl;
        IplImageTexture imageTexture;

        protected StatusStrip StatusStrip
        {
            get { return statusStrip; }
        }

        public override void Show(object value)
        {
            image = (IplImage)value;
            imageControl.Canvas.MakeCurrent();
            imageTexture.Update(image);
            imageControl.Canvas.Invalidate();
            canvasInvalidated = true;
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
            canvasInvalidated = false;
            imageControl = new VisualizerCanvasControl { Dock = DockStyle.Fill };
            statusStrip = new StatusStrip { Visible = false };
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            imageControl.RenderFrame += (sender, e) => RenderFrame();
            imageControl.SwapBuffers += (sender, e) => SwapBuffers();
            imageControl.Load += (sender, e) => imageTexture = new IplImageTexture();
            imageControl.Canvas.MouseClick += (sender, e) => statusStrip.Visible = e.Button == MouseButtons.Right ? !statusStrip.Visible : statusStrip.Visible;
            imageControl.Canvas.MouseDoubleClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Left)
                {
                    if (image != null)
                    {
                        imagePanel.Parent.ClientSize = new Size(image.Width, image.Height);
                    }
                }
            };

            imageControl.Canvas.MouseMove += (sender, e) =>
            {
                if (image != null && statusStrip.Visible)
                {
                    var cursorPosition = imageControl.Canvas.PointToClient(Form.MousePosition);
                    if (imageControl.ClientRectangle.Contains(cursorPosition))
                    {
                        var imageX = (int)(cursorPosition.X * ((float)image.Width / imageControl.Width));
                        var imageY = (int)(cursorPosition.Y * ((float)image.Height / imageControl.Height));
                        var cursorColor = Core.cvGet2D(image, imageY, imageX);
                        statusLabel.Text = string.Format("Cursor: ({0},{1}) Value: ({2},{3},{4})", imageX, imageY, cursorColor.Val0, cursorColor.Val1, cursorColor.Val2);
                    }
                }
            };

            imagePanel = new Panel { Dock = DockStyle.Fill, Size = new Size(320, 240) };
            imagePanel.Controls.Add(imageControl);
            imagePanel.Controls.Add(statusStrip);

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(imagePanel);
            }
        }

        public override IObservable<object> Visualize(IObservable<object> source, IServiceProvider provider)
        {
            var visualizerDialog = (TypeVisualizerDialog)provider.GetService(typeof(TypeVisualizerDialog));
            if (visualizerDialog != null)
            {
                return source
                    .Where(xs => allowUpdate)
                    .Do(xs => allowUpdate = false)
                    .ObserveOn(imageControl.Canvas)
                    .Do(xs => Show(xs));
            }

            return base.Visualize(source, provider);
        }

        public override void Unload()
        {
            imageTexture.Dispose();
            imagePanel.Dispose();
            imagePanel = null;
            statusStrip = null;
            imageControl = null;
            imageTexture = null;
            image = null;
        }
    }
}
