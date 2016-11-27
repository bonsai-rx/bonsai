﻿using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing.Design;
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
        [XmlArrayItem(typeof(EnableState))]
        [XmlArrayItem(typeof(DisableState))]
        [XmlArrayItem(typeof(ViewportState))]
        [XmlArrayItem(typeof(LineWidthState))]
        [XmlArrayItem(typeof(PointSizeState))]
        [XmlArrayItem(typeof(DepthMaskState))]
        [XmlArrayItem(typeof(BlendFunctionState))]
        [XmlArrayItem(typeof(DepthFunctionState))]
        [Description("Specifies any render states that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [Category("State")]
        [Description("Specifies any attachments that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.FramebufferAttachmentConfigurationCollectionEditor, Bonsai.Shaders.Design", "System.Drawing.Design.UITypeEditor, System.Drawing, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a")]
        public Collection<FramebufferAttachmentConfiguration> FramebufferAttachments
        {
            get { return framebufferConfiguration.FramebufferAttachments; }
        }

        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var framebuffer = new FramebufferState(framebufferConfiguration);
                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            framebuffer.Load(window);
                        });
                    }),
                    (input, window) =>
                    {
                        foreach (var state in renderState)
                        {
                            state.Execute(window);
                        }

                        framebuffer.Bind(window);
                        foreach (var material in window.Materials)
                        {
                            material.Draw();
                        }

                        framebuffer.Unbind(window);
                        return input;
                    }).Finally(() => framebuffer.Unload(null));
            });
        }
    }
}
