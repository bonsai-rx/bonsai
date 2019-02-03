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
    [Description("Produces a sequence of events whenever it is time to update a frame.")]
    [Editor("Bonsai.Shaders.Configuration.Design.ShaderConfigurationComponentEditor, Bonsai.Shaders.Design", typeof(ComponentEditor))]
    public class UpdateFrame : Source<FrameEvent>
    {
        public override IObservable<FrameEvent> Generate()
        {
            return ShaderManager.WindowSource.SelectMany(window => window.UpdateFrameAsync);
        }
    }
}
