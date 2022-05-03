using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that updates the render state of the shader window.
    /// </summary>
    [DefaultProperty(nameof(RenderState))]
    [Description("Updates the render state of the shader window.")]
    public class UpdateRenderState : Sink
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        /// <summary>
        /// Gets the collection of configuration objects specifying the render
        /// states to assign for subsequent operations.
        /// </summary>
        [Category("State")]
        [Description("Specifies a set of render states to assign for subsequent operations.")]
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

        /// <summary>
        /// Updates the render state of each shader window in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of shader windows to update.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// render state of each shader window in the sequence.
        /// </returns>
        public IObservable<ShaderWindow> Process(IObservable<ShaderWindow> source)
        {
            return source.Do(Process);
        }

        /// <summary>
        /// Updates the render state of the shader window contained in each set
        /// of resources in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of <see cref="ResourceConfigurationCollection"/> objects
        /// containing the shader windows to update.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// render state of the shader window contained in each set of resources
        /// in the sequence.
        /// </returns>
        public IObservable<ResourceConfigurationCollection> Process(IObservable<ResourceConfigurationCollection> source)
        {
            return source.Do(input =>
            {
                var windowManager = input.ResourceManager.Load<WindowManager>(string.Empty);
                Process(windowManager.Window);
            });
        }

        /// <summary>
        /// Updates the render state of the shader window whenever an observable
        /// sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to update the render
        /// state of the shader window.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of updating the
        /// render state of the shader window whenever the sequence emits a
        /// notification.
        /// </returns>
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
