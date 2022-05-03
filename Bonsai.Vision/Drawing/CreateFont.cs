using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    /// <summary>
    /// Represents an operator that creates a font which can be passed to
    /// text rendering functions.
    /// </summary>
    [Description("Creates a font which can be passed to text rendering functions.")]
    public class CreateFont : Source<Font>
    {
        Font value;
        event Action<Font> ValueChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="CreateFont"/> class.
        /// </summary>
        public CreateFont()
        {
            Font = SystemFonts.DefaultFont;
        }

        /// <summary>
        /// Gets or sets the font style used to render the text strokes.
        /// </summary>
        [XmlIgnore]
        [Description("The font style used to render the text strokes.")]
        public Font Font
        {
            get { return value; }
            set
            {
                this.value = value;
                OnValueChanged(value);
            }
        }

        /// <summary>
        /// Gets or sets an XML representation of the font for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(Font))]
        public string FontXml
        {
            get
            {
                var font = Font;
                if (font == null || font == SystemFonts.DefaultFont) return null;
                var converter = new FontConverter();
                return converter.ConvertToString(Font);
            }
            set
            {
                if (!string.IsNullOrEmpty(value))
                {
                    var converter = new FontConverter();
                    Font = (Font)converter.ConvertFromString(value);
                }
                else Font = SystemFonts.DefaultFont;
            }
        }

        void OnValueChanged(Font value)
        {
            ValueChanged?.Invoke(value);
        }

        /// <summary>
        /// Generates an observable sequence that contains an object representing
        /// a particular format for text, including font face and size.
        /// </summary>
        /// <returns>
        /// A sequence containing a single instance of the <see cref="System.Drawing.Font"/>
        /// class representing a particular format for text.
        /// </returns>
        public override IObservable<Font> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<Font>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }

        /// <summary>
        /// Generates an observable sequence of font objects representing a particular
        /// format for text, including font face and size, and where each font is
        /// emitted only when an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used for emitting new font
        /// objects.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="System.Drawing.Font"/> objects where each element
        /// represents a particular format for text.
        /// </returns>
        public IObservable<Font> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
