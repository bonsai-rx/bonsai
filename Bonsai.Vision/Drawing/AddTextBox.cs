﻿using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Drawing;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies rendering text strokes with the
    /// specified font and color inside the specified layout rectangle.
    /// </summary>
    [Description("Renders text strokes with the specified font and color inside the specified layout rectangle.")]
    public class AddTextBox : AddTextBase
    {
        /// <summary>
        /// Gets or sets the optional region in which to draw the text.
        /// By default the box will fill the entire image.
        /// </summary>
        [Description("The optional region in which to draw the text. By default the box will fill the entire image.")]
        public Rect Destination { get; set; }

        /// <summary>
        /// Returns the text rendering operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            return GetRenderer(Destination, (image, graphics, text, font, brush, format, rect) =>
            {
                if (rect.Width == 0) rect.Width = image.Width;
                if (rect.Height == 0) rect.Height = image.Height;
                graphics.DrawString(text, font, brush, new RectangleF(rect.X, rect.Y, rect.Width, rect.Height), format);
            });
        }
    }
}
