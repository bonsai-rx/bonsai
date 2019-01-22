using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Calculates the ratio of the window viewport width to its height.")]
    public class AspectRatio : Transform<Size, float>
    {
        static float GetAspectRatio(ShaderWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            var viewport = window.Viewport;
            return GetAspectRatio(viewport.Width * window.Width, viewport.Height * window.Height);
        }

        static float GetAspectRatio(float width, float height)
        {
            return width / height;
        }

        public override IObservable<float> Process(IObservable<Size> source)
        {
            return source.Select(input => GetAspectRatio(input.Width, input.Height));
        }

        public IObservable<float> Process(IObservable<INativeWindow> source)
        {
            return source.Select(input => GetAspectRatio((ShaderWindow)input));
        }

        public IObservable<float> Process<TEventArgs>(IObservable<EventPattern<INativeWindow, TEventArgs>> source)
        {
            return source.Select(input => GetAspectRatio((ShaderWindow)input.Sender));
        }

        public IObservable<float> Process(IObservable<Tuple<float, float>> source)
        {
            return source.Select(input => GetAspectRatio(input.Item1, input.Item2));
        }

        public IObservable<float> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) => GetAspectRatio(window));
        }
    }
}
