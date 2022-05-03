using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object for specifying the active viewport for
    /// rendering, in normalized coordinates.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ViewportState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets the x-coordinate of the lower left corner of the viewport,
        /// in normalized coordinates.
        /// </summary>
        [Description("The x-coordinate of the lower left corner of the viewport, in normalized coordinates.")]
        public float X { get; set; }

        /// <summary>
        /// Gets or sets the y-coordinate of the lower left corner of the viewport,
        /// in normalized coordinates.
        /// </summary>
        [Description("The y-coordinate of the lower left corner of the viewport, in normalized coordinates.")]
        public float Y { get; set; }

        /// <summary>
        /// Gets or sets the width of the viewport rectangle, in normalized coordinates.
        /// </summary>
        [Description("The width of the viewport rectangle, in normalized coordinates.")]
        public float Width { get; set; } = 1;

        /// <summary>
        /// Gets or sets the height of the viewport rectangle, in normalized coordinates.
        /// </summary>
        [Description("The height of the viewport rectangle, in normalized coordinates.")]
        public float Height { get; set; } = 1;

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            window.Viewport = new RectangleF(X, Y, Width, Height);
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"Viewport({X}, {Y}, {Width}, {Height})";
        }
    }
}
