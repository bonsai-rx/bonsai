using OpenTK;
using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Input
{
    public class NormalizedDeviceCoordinates : Transform<EventPattern<INativeWindow, MouseEventArgs>, Vector2>
    {
        static Vector2 ToNormalizedDeviceCoordinates(INativeWindow window, MouseEventArgs e)
        {
            var xpos = 2f * e.X / window.Width - 1;
            var ypos = -2f * e.Y / window.Height + 1;
            return new Vector2(xpos, ypos);
        }

        public override IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }

        public IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseButtonEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }

        public IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseMoveEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }

        public IObservable<Vector2> Process(IObservable<EventPattern<INativeWindow, MouseWheelEventArgs>> source)
        {
            return source.Select(evt => ToNormalizedDeviceCoordinates(evt.Sender, evt.EventArgs));
        }
    }
}
