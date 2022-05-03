using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    /// <summary>
    /// Represents an operator that generates a sequence with the current state
    /// of the mouse cursor. The position is defined in absolute desktop points,
    /// with the origin placed at the top-left corner of the display.
    /// </summary>
    [Description("Generates a sequence with the current state of the mouse cursor. The position is defined in absolute desktop points, with the origin placed at the top-left corner of the display.")]
    public class MouseCursor : Source<MouseState>
    {
        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the mouse cursor.
        /// </summary>
        /// <returns>
        /// A sequence of <see cref="MouseState"/> values representing the
        /// current state of the mouse cursor.
        /// </returns>
        public override IObservable<MouseState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => OpenTK.Input.Mouse.GetCursorState()));
        }

        /// <summary>
        /// Generates an observable sequence where each element represents the
        /// current state of the mouse cursor, at the time the <paramref name="source"/>
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications indicating when to check
        /// for the current state of the mouse cursor.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="MouseState"/> values representing the
        /// current state of the mouse cursor, at the time the
        /// <paramref name="source"/> sequence emits a notification.
        /// </returns>
        public IObservable<MouseState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => OpenTK.Input.Mouse.GetCursorState());
        }
    }
}
