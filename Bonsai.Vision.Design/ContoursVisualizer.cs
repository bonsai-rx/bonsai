using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Design;
using Bonsai.Vision;
using System.Windows.Forms;
using System.Drawing;

[assembly: TypeVisualizer(typeof(ContoursVisualizer), Target = typeof(Contours))]

namespace Bonsai.Vision.Design
{
    public class ContoursVisualizer : DialogTypeVisualizer
    {
        int maxLevel;
        int thickness;
        IplImageControl imageControl;

        public override void Show(object value)
        {
            var contours = (Contours)value;
            var output = new IplImage(contours.ImageSize, 8, 1);
            output.SetZero();

            if (!contours.FirstContour.IsInvalid)
            {
                Core.cvDrawContours(output, contours.FirstContour, CvScalar.All(255), CvScalar.All(128), maxLevel, thickness, 8, CvPoint.Zero);
            }

            imageControl.Image = output;
        }

        public override void Load(IServiceProvider provider)
        {
            maxLevel = 1;
            thickness = -1;
            imageControl = new IplImageControl { Dock = DockStyle.Fill };
            imageControl.Canvas.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    if (thickness < 0) thickness = 1;
                    else maxLevel = (maxLevel + 1) % 3;
                    if (maxLevel == 0)
                    {
                        maxLevel = 1;
                        thickness = -1;
                    }
                }
            };

            imageControl.Canvas.DoubleClick += (sender, e) =>
            {
                var image = imageControl.Image;
                if (image != null)
                {
                    imageControl.Parent.ClientSize = new Size(image.Width, image.Height);
                }
            };

            var visualizerService = (IDialogTypeVisualizerService)provider.GetService(typeof(IDialogTypeVisualizerService));
            if (visualizerService != null)
            {
                visualizerService.AddControl(imageControl);
            }
        }

        public override void Unload()
        {
            imageControl.Dispose();
            imageControl = null;
        }
    }
}
