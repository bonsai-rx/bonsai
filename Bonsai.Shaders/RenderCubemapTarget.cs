using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that renders all currently stored draw commands
    /// to a cubemap render target.
    /// </summary>
    [Description("Renders all currently stored draw commands to a cubemap render target.")]
    public class RenderCubemapTarget : Sink
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCubemapTarget"/> class.
        /// </summary>
        public RenderCubemapTarget()
        {
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
            TextureTarget = TextureTarget.TextureCubeMapNegativeZ;
        }

        /// <summary>
        /// Gets the collection of configuration objects specifying the render
        /// states to be set when rendering the cubemap.
        /// </summary>
        [Description("Specifies the set of render states to be set when rendering the cubemap.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before rendering.
        /// </summary>
        [XmlIgnore]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        /// <summary>
        /// Gets or sets an XML representation of the clear color for serialization.
        /// </summary>
        [Browsable(false)]
        [XmlElement(nameof(ClearColor))]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        /// <summary>
        /// Gets or sets a value specifying which buffers to clear before rendering.
        /// </summary>
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }

        /// <summary>
        /// Gets or sets the name of the cubemap texture to update.
        /// </summary>
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the cubemap texture to update.")]
        public string TextureName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying which cubemap texture target to update.
        /// </summary>
        [TypeConverter(typeof(CubemapTargetConverter))]
        [Description("Specifies which cubemap texture target to update.")]
        public TextureTarget TextureTarget { get; set; }

        /// <summary>
        /// Renders all currently stored draw commands to a cubemap render target
        /// whenever an observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to start rendering to the cubemap
        /// target.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the <paramref name="source"/>
        /// sequence but where there is an additional side effect of rendering the
        /// specified cubemap target whenever the sequence emits a notification.
        /// </returns>
        public override IObservable<TSource> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var fbo = 0;
                var faceSize = 0;
                var depthRenderbuffer = 0;
                var colorTarget = default(Texture);
                var name = TextureName;
                if (string.IsNullOrEmpty(name))
                {
                    throw new InvalidOperationException("A texture name must be specified.");
                }

                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            GL.GenFramebuffers(1, out fbo);
                            GL.GenRenderbuffers(1, out depthRenderbuffer);
                            colorTarget = window.ResourceManager.Load<Texture>(name);
                            GL.BindTexture(TextureTarget.TextureCubeMap, colorTarget.Id);
                            GL.GetTexLevelParameter(TextureTarget, 0, GetTextureParameter.TextureWidth, out faceSize);
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, faceSize, faceSize);
                            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget, colorTarget.Id, 0);
                            GL.BindTexture(TextureTarget.TextureCubeMap, 0);
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, 0);
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                        });
                    }),
                    (input, window) =>
                    {
                        foreach (var state in renderState)
                        {
                            state.Execute(window);
                        }

                        var clearMask = ClearMask;
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                        window.UpdateViewport(faceSize, faceSize);
                        window.UpdateScissor(faceSize, faceSize);
                        if (clearMask != ClearBufferMask.None)
                        {
                            GL.ClearColor(ClearColor);
                            GL.Clear(clearMask);
                        }

                        foreach (var shader in window.Shaders)
                        {
                            shader.Dispatch();
                        }

                        window.UpdateViewport();
                        window.UpdateScissor();
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
                        return input;
                    }).Finally(() =>
                    {
                        if (fbo > 0)
                        {
                            GL.DeleteFramebuffers(1, ref fbo);
                            GL.DeleteRenderbuffers(1, ref depthRenderbuffer);
                        }
                    });
            });
        }
    }
}
