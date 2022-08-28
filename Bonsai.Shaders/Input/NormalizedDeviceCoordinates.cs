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
    /// Represents an operator that converts each point in the sequence from window
    /// client coordinates into normalized device coordinates.
    /// </summary>
    [Description("Converts each point in the sequence from window client coordinates into normalized device coordinates.")]
    public class NormalizedDeviceCoordinates : Transform<EventPattern<INativeWindow, MouseEventArgs>, Vector2>
    {
        static Vector2 ToNormalizedDeviceCoordinates(INativeWindow window, MouseEventArgs e)
        {
            var xpos = 2f * e.X / window.Width - 1;
            var ypos = -2f * e.Y / window.Height + 1;
            return new Vector2(xpos, ypos);
        }

        /// <summary>
        /// Converts each point in an observable sequence of mouse device event data
        /// from window client coordinates into normalized device coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of events containing <see cref="MouseEventArgs"/> event data.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Vector2"/> values representing the normalized
        /// device coordinates corresponding to the window client location stored in
        /// the event data.
        /// </returns>
        public override IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }

        /// <summary>
        /// Converts each point in an observable sequence of mouse button event data
        /// from window client coordinates into normalized device coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of events containing <see cref="MouseButtonEventArgs"/> event data.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Vector2"/> values representing the normalized
        /// device coordinates corresponding to the window client location stored in
        /// the event data.
        /// </returns>
        public IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseButtonEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }

        /// <summary>
        /// Converts each point in an observable sequence of mouse move event data
        /// from window client coordinates into normalized device coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of events containing <see cref="MouseMoveEventArgs"/> event data.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Vector2"/> values representing the normalized
        /// device coordinates corresponding to the window client location stored in
        /// the event data.
        /// </returns>
        public IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseMoveEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }

        /// <summary>
        /// Converts each point in an observable sequence of mouse wheel event data
        /// from window client coordinates into normalized device coordinates.
        /// </summary>
        /// <param name="source">
        /// A sequence of events containing <see cref="MouseWheelEventArgs"/> event data.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="Vector2"/> values representing the normalized
        /// device coordinates corresponding to the window client location stored in
        /// the event data.
        /// </returns>
        public IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseWheelEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }
    }
}
