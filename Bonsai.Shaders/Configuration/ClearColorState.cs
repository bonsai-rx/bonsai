using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Represents a configuration object specifying the color used to clear
    /// the framebuffer.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ClearColorState : StateConfiguration
    {
        /// <summary>
        /// Gets or sets a value specifying the color used to clear the framebuffer
        /// before rendering.
        /// </summary>
        [XmlIgnore]
        [Description("Specifies the color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets an HTML representation of the clear color value for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(ClearColor))]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        /// <inheritdoc/>
        public override void Execute(ShaderWindow window)
        {
            window.ClearColor = ClearColor;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return $"ClearColor({ClearColor})";
        }
    }
}
