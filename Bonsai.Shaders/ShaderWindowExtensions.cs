using OpenTK;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    static class ShaderWindowExtensions
    {
        public static IObservable<EventPattern<INativeWindow, TEventArgs>> EventPattern<TEventArgs>(
            this INativeWindow window,
            Action<EventHandler<TEventArgs>> addHandler,
            Action<EventHandler<TEventArgs>> removeHandler)
        {
            return Observable.Create<EventPattern<INativeWindow, TEventArgs>>(observer =>
            {
                EventHandler<EventArgs> closed = (sender, e) => observer.OnCompleted();
                EventHandler<TEventArgs> handler = (sender, e) => observer.OnNext(
                    new EventPattern<INativeWindow, TEventArgs>(window, e));
                addHandler(handler);
                window.Closed += closed;
                return Disposable.Create(() =>
                {
                    window.Closed -= closed;
                    removeHandler(handler);
                });
            });
        }
    }
}
