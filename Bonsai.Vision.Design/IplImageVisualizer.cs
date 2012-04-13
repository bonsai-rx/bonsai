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
        Panel imagePanel;
        StatusStrip statusStrip;
        ToolStripStatusLabel statusLabel;
        IplImageControl imageControl;

        protected StatusStrip StatusStrip
        {
            get { return statusStrip; }
        }

        private string UpdateImageStatus(IplImage image)
        {
            if (image != null)
            {
                var cursorPosition = imageControl.PictureBox.PointToClient(Form.MousePosition);
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
            var image = (IplImage)value;
            if (statusStrip.Visible)
            {
                statusLabel.Text = UpdateImageStatus(image);
            }

            imageControl.Image = image;
        }

        public override void Load(IServiceProvider provider)
        {
            imageControl = new IplImageControl();
            statusStrip = new StatusStrip { Visible = false };
            statusLabel = new ToolStripStatusLabel();
            statusStrip.Items.Add(statusLabel);
            imageControl.PictureBox.MouseClick += (sender, e) => statusStrip.Visible = e.Button == MouseButtons.Right ? !statusStrip.Visible : statusStrip.Visible;
            imageControl.PictureBox.DoubleClick += (sender, e) =>
            {
                if (imageControl.Image != null)
                {
                    imagePanel.Parent.ClientSize = new Size(imageControl.Image.Width, imageControl.Image.Height);
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
