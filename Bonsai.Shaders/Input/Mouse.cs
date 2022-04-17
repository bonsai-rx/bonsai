using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence with the current state
    /// of the specified mouse device.
    /// </summary>
    /// <remarks>
    /// The position and wheel values are defined in a hardware-specific coordinate system.
    /// </remarks>
    [DefaultProperty(nameof(Index))]
    [Description("Generates a sequence with the current state of the specified mouse device.")]
    public class Mouse : Source<MouseState>
    {
        /// <summary>
        /// Gets or sets the index of the mouse device. If it is not specified,
        /// the combined state of all devices is retrieved.
        /// </summary>
        [Description("The index of the mouse device. If it is not specified, the combined state of all devices is retrieved.")]
        public int? Index { get; set; }

        static MouseState GetMouseState(int? index)
        {
            if (index.HasValue) return OpenTK.Input.Mouse.GetState(index.Value);
            else return OpenTK.Input.Mouse.GetState();
        }

        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the specified mouse device.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="MouseState"/> values representing the
        /// current state of the mouse device.
        /// </returns>
        public override IObservable<MouseState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => GetMouseState(Index)));
        }

        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the specified mouse device, at the time the
        /// <paramref name="source"/> sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications indicating when to check
        /// for the current state of the mouse device.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="MouseState"/> values representing the
        /// current state of the mouse device, at the time the
        /// <paramref name="source"/> sequence emits a notification.
        /// </returns>
        public IObservable<MouseState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => GetMouseState(Index));
        }
    }
}
