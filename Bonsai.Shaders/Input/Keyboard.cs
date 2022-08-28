using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence with the current state
    /// of the specified keyboard device.
    /// </summary>
    [DefaultProperty(nameof(Index))]
    [Description("Generates a sequence with the current state of the specified keyboard device.")]
    public class Keyboard : Source<KeyboardState>
    {
        /// <summary>
        /// Gets or sets the index of the keyboard device. If it is not specified,
        /// the combined state of all devices is retrieved.
        /// </summary>
        [Description("The index of the keyboard device. If it is not specified, the combined state of all devices is retrieved.")]
        public int? Index { get; set; }

        static KeyboardState GetKeyboardState(int? index)
        {
            if (index.HasValue) return OpenTK.Input.Keyboard.GetState(index.Value);
            else return OpenTK.Input.Keyboard.GetState();
        }

        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the specified keyboard device.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="KeyboardState"/> values representing the
        /// current state of the keyboard device.
        /// </returns>
        public override IObservable<KeyboardState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => GetKeyboardState(Index)));
        }

        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the specified keyboard device, at the time the
        /// <paramref name="source"/> sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications indicating when to check
        /// for the current state of the keyboard device.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="KeyboardState"/> values representing the
        /// current state of the keyboard device, at the time the
        /// <paramref name="source"/> sequence emits a notification.
        /// </returns>
        public IObservable<KeyboardState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => GetKeyboardState(Index));
        }
    }
}
