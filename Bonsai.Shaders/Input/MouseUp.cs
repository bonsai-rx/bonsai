﻿using OpenTK.Input;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Input
{
    [Description("Produces a sequence of events whenever a mouse button is released over the shader window.")]
    [Editor("Bonsai.Shaders.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class MouseUp : Source<EventPattern<MouseButtonEventArgs>>
    {
        public override IObservable<EventPattern<MouseButtonEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<MouseButtonEventArgs>(
                handler => window.MouseUp += handler,
                handler => window.MouseUp -= handler)
                .TakeUntil(window.WindowClosed()));
        }
    }
}
