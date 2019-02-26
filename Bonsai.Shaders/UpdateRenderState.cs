using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [DefaultProperty("RenderState")]
    [Description("Updates the render state of the shader window.")]
    public class UpdateRenderState : Sink
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        [Category("State")]
        [Description("Specifies the render states that should be assigned on the shader window.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        public IObservable<ShaderWindow> Process(IObservable<ShaderWindow> source)
        {
            return source.Do(window =>
            {
                foreach (var state in renderState)
                {
                    state.Execute(window);
                }
            });
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) =>
                {
                    foreach (var state in renderState)
                    {
                        state.Execute(window);
                    }
                    return input;
                });
        }
    }
}
