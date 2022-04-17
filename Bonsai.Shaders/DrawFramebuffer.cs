using Bonsai.Shaders.Configuration;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that renders all currently stored draw commands
    /// to a framebuffer.
    /// </summary>
    [Description("Renders all currently stored draw commands to a framebuffer.")]
    public class DrawFramebuffer : Sink
    {
        readonly FramebufferConfiguration framebufferConfiguration = new FramebufferConfiguration();
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        /// <summary>
        /// Gets a collection of state configuration objects specifying any
        /// render states that are required to render the framebuffer.
        /// </summary>
        [Category("State")]
        [Description("Specifies any render states that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        /// <summary>
        /// Gets a collection of configuration objects specifying any attachments
        /// that are required to render the framebuffer.
        /// </summary>
        [Category("State")]
        [Description("Specifies any attachments that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebufferConfiguration.FramebufferAttachments; }
        }

        /// <summary>
        /// Renders all currently stored draw commands to a framebuffer whenever
        /// an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications used to start rendering all
        /// stored draw commands to a framebuffer.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of rendering all
        /// stored draw commands to a framebuffer whenever the sequence emits a
        /// notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                FramebufferState framebuffer = null;
                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        framebuffer = new FramebufferState(window, framebufferConfiguration);
                    }),
                    (input, window) =>
                    {
                        foreach (var state in renderState)
                        {
                            state.Execute(window);
                        }

                        framebuffer.Bind();
                        foreach (var shader in window.Shaders)
                        {
                            shader.Dispatch();
                        }

                        framebuffer.Unbind();
                        return input;
                    }).Finally(() =>
                    {
                        if (framebuffer != null)
                        {
                            framebuffer.Dispose();
                        }
                    });
            });
        }
    }
}
