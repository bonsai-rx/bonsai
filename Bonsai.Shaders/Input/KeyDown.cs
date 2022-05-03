using OpenTK;
using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence of events produced whenever
    /// a key is pressed while the shader window has focus.
    /// </summary>
    [DefaultProperty(nameof(Key))]
    [Description("Generates a sequence of events produced whenever a key is pressed while the shader window has focus.")]
    public class KeyDown : Source<EventPattern<INativeWindow, KeyboardKeyEventArgs>>
    {
        /// <summary>
        /// Gets or sets a value specifying an optional key to use as a filter.
        /// </summary>
        [TypeConverter(typeof(NullableEnumConverter))]
        [Description("Specifies an optional key to use as a filter.")]
        public Key? Key { get; set; }

        /// <summary>
        /// Gets or sets a value specifying optional key modifiers to use as
        /// a filter.
        /// </summary>
        [TypeConverter(typeof(NullableEnumConverter))]
        [Description("Specifies optional key modifiers to use as a filter.")]
        public KeyModifiers? Modifiers { get; set; }

        /// <summary>
        /// Generates an observable sequence that produces a value whenever a key
        /// is pressed while the shader window has focus.
        /// </summary>
        /// <returns>
        /// A sequence of events containing <see cref="KeyboardKeyEventArgs"/> event
        /// data produced whenever a key is pressed while the shader window has focus.
        /// </returns>
        public override IObservable<EventPattern<INativeWindow, KeyboardKeyEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<KeyboardKeyEventArgs>(
                handler => window.KeyDown += handler,
                handler => window.KeyDown -= handler))
                .Where(evt =>
                {
                    var args = evt.EventArgs;
                    var key = Key.GetValueOrDefault(args.Key);
                    var modifiers = Modifiers.GetValueOrDefault(args.Modifiers);
                    return args.Key == key && args.Modifiers == modifiers;
                });
        }
    }
}
