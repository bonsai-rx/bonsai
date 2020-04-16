using System;
using System.Collections.Generic;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using System.Windows.Forms;
using System.Drawing;
using Size = System.Drawing.Size;

[assembly: TypeVisualizer(typeof(IplImageVisualizer), Target = typeof(IplImage))]

namespace Bonsai.Vision.Design
{
    public class IplImageVisualizer : ImageMashupVisualizer
    {
        UserControl imagePanel;
        StatusStrip statusStrip;
        ToolStripStatusLabel statusLabel;
        VisualizerCanvas visualizerCanvas;
        IplImageTexture imageTexture;

        protected bool StatusStripEnabled { get; set; }

        protected StatusStrip StatusStrip
        {
            get { return statusStrip; }
        }

        public override VisualizerCanvas VisualizerCanvas
        {
            get { return visualizerCanvas; }
        }

        protected override void ShowMashup(IList<object> values)
        {
            base.ShowMashup(values);
            visualizerCanvas.MakeCurrent();
            if (VisualizerImage != null)
            {
                imageTexture.Update(VisualizerImage);
            }
            visualizerCanvas.Canvas.Invalidate();
        }

        public override void Show(object value)
        {
            base.Show(value);
            UpdateStatus();
        }

        protected virtual void RenderFrame()
        {
            imageTexture.Draw();
        }

        private void UpdateStatus()
        {
            var visualizerImage = VisualizerImage;
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
                    if (VisualizerImage != null)
                    {
                        imagePanel.Parent.ClientSize = new Size(VisualizerImage.Width, VisualizerImage.Height);
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
                visualizerService.AddControl(imagePanel);
            }

            base.Load(provider);
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
        }
    }
}
