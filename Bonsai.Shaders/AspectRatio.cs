﻿using OpenTK;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class AspectRatio : Transform<Size, float>
    {
        float GetAspectRatio(float width, float height)
        {
            return width / height;
        }

        public override IObservable<float> Process(IObservable<Size> source)
        {
            return source.Select(input => GetAspectRatio(input.Width, input.Height));
        }

        public IObservable<float> Process(IObservable<INativeWindow> source)
        {
            return source.Select(input => GetAspectRatio(input.Width, input.Height));
        }

        public IObservable<float> Process<TEventArgs>(IObservable<EventPattern<INativeWindow, TEventArgs>> source)
        {
            return source.Select(input => GetAspectRatio(input.Sender.Width, input.Sender.Height));
        }

        public IObservable<float> Process(IObservable<Tuple<float, float>> source)
        {
            return source.Select(input => GetAspectRatio(input.Item1, input.Item2));
        }
    }
}
