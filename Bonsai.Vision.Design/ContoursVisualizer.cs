﻿using System;
using Bonsai;
using Bonsai.Vision.Design;
using OpenCV.Net;
using Bonsai.Vision;
using System.Windows.Forms;

[assembly: TypeVisualizer(typeof(ContoursVisualizer), Target = typeof(Contours))]

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a type visualizer that displays the outline of all the polygonal
    /// contours in a hierarchy.
    /// </summary>
    public class ContoursVisualizer : IplImageVisualizer
    {
        int thickness;

        /// <inheritdoc/>
        public override void Show(object value)
        {
            var contours = (Contours)value;
            var output = new IplImage(contours.ImageSize, IplDepth.U8, 1);
            output.SetZero();

            if (contours.FirstContour != null)
            {
                CV.DrawContours(output, contours.FirstContour, Scalar.All(255), Scalar.All(128), 2, thickness);
            }

            base.Show(output);
        }

        /// <inheritdoc/>
        public override void Load(IServiceProvider provider)
        {
            thickness = -1;
            base.Load(provider);
            StatusStripEnabled = false;
            VisualizerCanvas.Canvas.MouseClick += (sender, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    thickness *= -1;
                }
            };
        }
    }
}
