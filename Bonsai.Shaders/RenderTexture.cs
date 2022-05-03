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
    /// to a texture.
    /// </summary>
    [Description("Renders all currently stored draw commands to a texture.")]
    public class RenderTexture : Combinator<Texture>
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderTexture"/> class.
        /// </summary>
        public RenderTexture()
        {
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
            WrapS = TextureWrapMode.Repeat;
            WrapT = TextureWrapMode.Repeat;
            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMagFilter.Linear;
            InternalFormat = PixelInternalFormat.Rgba;
        }

        /// <summary>
        /// Gets the collection of configuration objects specifying the render
        /// states to be set when rendering the texture.
        /// </summary>
        [Category("Render Settings")]
        [Description("Specifies the set of render states to be set when rendering the texture.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        /// <summary>
        /// Gets or sets the color used to clear the framebuffer before rendering.
        /// </summary>
        [XmlIgnore]
        [Category("Render Settings")]
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
        [Category("Render Settings")]
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }

        /// <summary>
        /// Gets or sets the width of the texture. If no value is specified, the
        /// texture buffer will be initialized to the width of the shader window.
        /// </summary>
        [Category("TextureSize")]
        [Description("The width of the texture. If no value is specified, the texture buffer will be initialized to the size of the shader window.")]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or sets the height of the texture. If no value is specified, the
        /// texture buffer will be initialized to the height of the shader window.
        /// </summary>
        [Category("TextureSize")]
        [Description("The height of the texture. If no value is specified, the texture buffer will be initialized to the height of the shader window.")]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the internal storage format of the
        /// render texture.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the internal storage format of the texture target.")]
        public PixelInternalFormat InternalFormat { get; set; }

        /// <summary>
        /// Gets or sets a value specifying wrapping parameters for the column
        /// coordinates of the texture sampler.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the column coordinates of the texture sampler.")]
        public TextureWrapMode WrapS { get; set; }

        /// <summary>
        /// Gets or sets a value specifying wrapping parameters for the row
        /// coordinates of the texture sampler.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the row coordinates of the texture sampler.")]
        public TextureWrapMode WrapT { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the texture minification filter.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the texture magnification filter.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the texture magnification filter.")]
        public TextureMagFilter MagFilter { get; set; }

        Texture CreateRenderTarget(int width, int height, PixelInternalFormat internalFormat, PixelFormat format, PixelType type)
        {
            var texture = new Texture();
            GL.BindTexture(TextureTarget.Texture2D, texture.Id);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)WrapS);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)WrapT);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)MinFilter);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)MagFilter);
            GL.TexImage2D(
                TextureTarget.Texture2D, 0,
                internalFormat, width, height, 0,
                format,
                type,
                IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);
            return texture;
        }

        /// <summary>
        /// Renders all currently stored draw commands to a texture whenever an
        /// observable sequence emits a notification.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to start the render to texture.
        /// </param>
        /// <returns>
        /// A sequence returning the <see cref="Texture"/> object representing the
        /// render target, whenever the render to texture operation completes.
        /// </returns>
        public override IObservable<Texture> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var fbo = 0;
                var width = 0;
                var height = 0;
                var depthRenderbuffer = 0;
                var colorTarget = default(Texture);
                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            width = Width.GetValueOrDefault(window.Width);
                            height = Height.GetValueOrDefault(window.Height);
                            GL.GenFramebuffers(1, out fbo);
                            GL.GenRenderbuffers(1, out depthRenderbuffer);
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, width, height);
                            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                            colorTarget = CreateRenderTarget(width, height, InternalFormat, PixelFormat.Rgba, PixelType.UnsignedByte);
                            GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, colorTarget.Id, 0);
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
                        window.UpdateViewport(width, height);
                        window.UpdateScissor(width, height);
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
                        return colorTarget;
                    }).Finally(() =>
                    {
                        if (fbo > 0)
                        {
                            GL.DeleteFramebuffers(1, ref fbo);
                            GL.DeleteRenderbuffers(1, ref depthRenderbuffer);
                            colorTarget.Dispose();
                        }
                    });
            });
        }
    }
}
