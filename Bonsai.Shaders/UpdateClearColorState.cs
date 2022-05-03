using System;
using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that updates the clear color state of the
    /// shader window.
    /// </summary>
    [Description("Updates the clear color state of the shader window.")]
    public class UpdateClearColorState : Sink
    {
        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before rendering.
        /// </summary>
        [XmlIgnore]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; } = Color.Black;

        /// <summary>
        /// Gets or sets an XML representation of the clear color for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(ClearColor))]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        /// <summary>
        /// Updates the clear color state of the shader window whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to update the clear
        /// color state of the shader window.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// clear color of the shader window whenever the sequence emits a notification.
        /// </returns>
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
