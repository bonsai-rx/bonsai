using OpenTK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Produces a sequence of events whenever it is time to update a frame.")]
    [Editor("Bonsai.Shaders.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class UpdateFrame : Source<double>
    {
        public override IObservable<double> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => Observable.FromEventPattern<FrameEventArgs>(
                handler => window.UpdateFrame += handler,
                handler => window.UpdateFrame -= handler)
                .Select(evt => evt.EventArgs.Time));
        }
    }
}
