using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [Description("Updates the clear color state of the shader window.")]
    public class UpdateClearColorState : Sink
    {
        public UpdateClearColorState()
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

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) =>
                {
                    window.ClearColor = ClearColor;
                    return input;
                });
        }
    }
}
