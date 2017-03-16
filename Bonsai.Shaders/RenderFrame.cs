using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Produces a sequence of events whenever it is time to render a frame.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class RenderFrame : Source<EventPattern<INativeWindow, FrameEventArgs>>
    {
        public override IObservable<EventPattern<INativeWindow, FrameEventArgs>> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.EventPattern<FrameEventArgs>(
                handler => window.RenderFrame += handler,
                handler => window.RenderFrame -= handler));
        }
    }
}
