using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    [DefaultProperty(nameof(RenderState))]
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

        private void Process(ShaderWindow window)
        {
            foreach (var state in renderState)
            {
                state.Execute(window);
            }
        }

        public IObservable<ShaderWindow> Process(IObservable<ShaderWindow> source)
        {
            return source.Do(Process);
        }

        public IObservable<ResourceConfigurationCollection> Process(IObservable<ResourceConfigurationCollection> source)
        {
            return source.Do(input =>
            {
                var windowManager = input.ResourceManager.Load<WindowManager>(string.Empty);
                Process(windowManager.Window);
            });
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return source.CombineEither(
                ShaderManager.WindowSource,
                (input, window) =>
                {
                    Process(window);
                    return input;
                });
        }
    }
}
