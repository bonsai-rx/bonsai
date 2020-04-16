using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Vision.Drawing
{
    [Description("Creates a font that can be passed to text rendering functions.")]
    public class CreateFont : Source<Font>
    {
        Font value;
        event Action<Font> ValueChanged;

        public CreateFont()
        {
            Font = SystemFonts.DefaultFont;
        }

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

        [Browsable(false)]
        [XmlElement("Font")]
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
            var handler = ValueChanged;
            if (handler != null)
            {
                handler(value);
            }
        }

        public override IObservable<Font> Generate()
        {
            return Observable
                .Defer(() => Observable.Return(value))
                .Concat(Observable.FromEvent<Font>(
                    handler => ValueChanged += handler,
                    handler => ValueChanged -= handler));
        }

        public IObservable<Font> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(x => value);
        }
    }
}
