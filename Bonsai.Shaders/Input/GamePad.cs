﻿using OpenTK.Input;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Shaders.Input
{
    [DefaultProperty(nameof(Index))]
    [Description("Retrieves the state of the specified gamepad device.")]
    public class GamePad : Source<GamePadState>
    {
        [Description("The index of the gamepad device.")]
        public int Index { get; set; }

        public override IObservable<GamePadState> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync
                .Select(evt => OpenTK.Input.GamePad.GetState(Index)));
        }

        public IObservable<GamePadState> Generate<TSource>(IObservable<TSource> source)
        {
            return source.Select(input => OpenTK.Input.GamePad.GetState(Index));
        }
    }
}
