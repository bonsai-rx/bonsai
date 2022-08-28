using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence with the current state
    /// of the specified gamepad device.
    /// </summary>
    [DefaultProperty(nameof(Index))]
    [Description("Generates a sequence with the current state of the specified gamepad device.")]
    public class GamePad : Source<GamePadState>
    {
        /// <summary>
        /// Gets or sets the index of the gamepad device.
        /// </summary>
        [Description("The index of the gamepad device.")]
        public int Index { get; set; }

        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the specified gamepad device.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="GamePadState"/> values representing the
        /// current state of the gamepad device.
        /// </returns>
        public override IObservable<GamePadState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => OpenTK.Input.GamePad.GetState(Index)));
        }

        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the specified gamepad device, at the time the
        /// <paramref name="source"/> sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications indicating when to check
        /// for the current state of the gamepad device.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="GamePadState"/> values representing the
        /// current state of the gamepad device, at the time the
        /// <paramref name="source"/> sequence emits a notification.
        /// </returns>
        public IObservable<GamePadState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => OpenTK.Input.GamePad.GetState(Index));
        }
    }
}
