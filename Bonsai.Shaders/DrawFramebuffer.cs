using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [Description("Renders all currently stored draw commands to a framebuffer.")]
    public class DrawFramebuffer : Sink
    {
        readonly FramebufferConfiguration framebufferConfiguration = new FramebufferConfiguration();
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        [Category("State")]
        [Description("Specifies any render states that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("State")]
        [Description("Specifies any attachments that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebufferConfiguration.FramebufferAttachments; }
        }

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
