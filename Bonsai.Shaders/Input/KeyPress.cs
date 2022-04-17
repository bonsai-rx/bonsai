using OpenTK;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence of characters produced
    /// whenever a key is pressed while the shader window has focus.
    /// </summary>
    [DefaultProperty(nameof(KeyChar))]
    [Description("Generates a sequence of characters produced whenever a key is pressed while the shader window has focus.")]
    public class KeyPress : Source<EventPattern<INativeWindow, KeyPressEventArgs>>
    {
        /// <summary>
        /// Gets or sets a value specifying an optional character to use as a filter.
        /// </summary>
        [Description("Specifies an optional character to use as a filter.")]
        public char? KeyChar { get; set; }

        /// <summary>
        /// Generates an observable sequence that produces a character whenever the
        /// corresponding key is pressed while the shader window has focus.
        /// </summary>
        /// <returns>
        /// A sequence of events containing <see cref="KeyPressEventArgs"/> event
        /// data produced whenever a character key is pressed while the shader window
        /// has focus.
        /// </returns>
        public override IObservable<EventPattern<INativeWindow, KeyPressEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<KeyPressEventArgs>(
                handler => window.KeyPress += handler,
                handler => window.KeyPress -= handler))
                .Where(evt => 
                {
                    var args = evt.EventArgs;
                    var key = KeyChar.GetValueOrDefault(args.KeyChar);
                    return args.KeyChar == key;
                });
        }
    }
}
