using OpenCV.Net;
using System;
using System.ComponentModel;
using Point = OpenCV.Net.Point;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that specifies rendering text strokes with the
    /// specified font and color at a given location.
    /// </summary>
    [Description("Renders text strokes with the specified font and color at a given location.")]
    public class AddText : AddTextBase
    {
        /// <summary>
        /// Gets or sets the coordinates of the upper-left corner of the drawn text.
        /// </summary>
        [Description("The coordinates of the upper-left corner of the drawn text.")]
        public Point Origin { get; set; }

        /// <summary>
        /// Returns the text rendering operation.
        /// </summary>
        /// <inheritdoc/>
        protected override Action<IplImage> GetRenderer()
        {
            return GetRenderer(Origin, (image, graphics, text, font, brush, format, origin) =>
            {
                graphics.DrawString(text, font, brush, origin.X, origin.Y, format);
            });
        }
    }
}
