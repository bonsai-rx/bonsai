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

[assembly: TypeVisualizer(typeof(IplImageVisualizer), Target = typeof(IplImage))]

namespace Bonsai.Vision.Design
{
    public class IplImageVisualizer : DialogTypeVisualizer
    {
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

        private string UpdateImageStatus(IplImage image)
        {
            if (image != null)
            {
                var cursorPosition = imageControl.Canvas.PointToClient(Form.MousePosition);
                if (imageControl.ClientRectangle.Contains(cursorPosition))
                {
                    var imageX = (int)(cursorPosition.X * ((float)image.Width / imageControl.Width));
                    var imageY = (int)(cursorPosition.Y * ((float)image.Height / imageControl.Height));
                    var cursorColor = Core.cvGet2D(image, imageY, imageX);
                    return string.Format("Cursor: ({0},{1}) Value: ({2},{3},{4})", imageX, imageY, cursorColor.Val0, cursorColor.Val1, cursorColor.Val2);
                }
            }

            return string.Empty;
        }

        public override void Show(object value)
        {
            image = (IplImage)value;
            if (statusStrip.Visible)
            {
                statusLabel.Text = UpdateImageStatus(image);
            }

            imageControl.Canvas.MakeCurrent();
            imageTexture.Update(image);
            imageControl.Canvas.Invalidate();
        }

        protected virtual void RenderFrame()
        {
            imageTexture.Draw();
        }

        public override void Load(IServiceProvider provider)
        {
            imageControl = new IplImageControl { Dock = DockStyle.Fill };
            statusStrip = new StatusStrip { Visible = false };
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            imageControl.RenderFrame += (sender, e) => RenderFrame();
            imageControl.Load += (sender, e) => imageTexture = new IplImageTexture();
            imageControl.Canvas.MouseClick += (sender, e) => statusStrip.Visible = e.Button == MouseButtons.Right ? !statusStrip.Visible : statusStrip.Visible;
            imageControl.Canvas.DoubleClick += (sender, e) =>
            {
                if (image != null)
                {
                    imagePanel.Parent.ClientSize = new Size(image.Width, image.Height);
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

        public override void Unload()
        {
            imagePanel.Dispose();
            imagePanel = null;
            statusStrip = null;
            imageControl = null;
        }
    }
}
