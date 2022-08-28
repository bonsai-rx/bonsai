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
    /// a mouse button is pressed over the shader window.
    /// </summary>
    [DefaultProperty(nameof(Button))]
    [Description("Generates a sequence of events produced whenever a mouse button is pressed over the shader window.")]
    public class MouseDown : Source<EventPattern<INativeWindow, MouseButtonEventArgs>>
    {
        /// <summary>
        /// Gets or sets a value specifying an optional mouse button to use as
        /// a filter.
        /// </summary>
        [TypeConverter(typeof(NullableEnumConverter))]
        [Description("Specifies an optional mouse button to use as a filter.")]
        public MouseButton? Button { get; set; }

        /// <summary>
        /// Generates an observable sequence that produces a value whenever a mouse
        /// button is pressed over the shader window.
        /// </summary>
        /// <returns>
        /// A sequence of events containing <see cref="MouseButtonEventArgs"/> event
        /// data produced whenever a mouse button is pressed over the shader window.
        /// </returns>
        public override IObservable<EventPattern<INativeWindow, MouseButtonEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<MouseButtonEventArgs>(
                handler => window.MouseDown += handler,
                handler => window.MouseDown -= handler))
                .Where(evt =>
                {
                    var args = evt.EventArgs;
                    var button = Button.GetValueOrDefault(args.Button);
                    return args.Button == button;
                });
        }
    }
}
