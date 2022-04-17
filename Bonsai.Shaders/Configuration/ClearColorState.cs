using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class ClearColorState : StateConfiguration
    {
        public ClearColorState()
        {
            ClearColor = Color.Black;
        }

        [XmlIgnore]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement(nameof(ClearColor))]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        public override void Execute(ShaderWindow window)
        {
            window.ClearColor = ClearColor;
        }

        public override string ToString()
        {
            return $"ClearColor({ClearColor})";
        }
    }
}
