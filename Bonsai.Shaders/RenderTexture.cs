using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    [Description("Renders all currently stored draw commands to a texture.")]
    public class RenderTexture : Combinator<Texture>
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

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

        [Category("Render Settings")]
        [Description("Specifies any render states that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [XmlIgnore]
        [Category("Render Settings")]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        [Category("Render Settings")]
        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }

        [Category("TextureSize")]
        [Description("The optional width of the texture.")]
        public int? Width { get; set; }

        [Category("TextureSize")]
        [Description("The optional height of the texture.")]
        public int? Height { get; set; }

        [Category("TextureParameter")]
        [Description("The internal storage format of the texture target.")]
        public PixelInternalFormat InternalFormat { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the column coordinates of the texture sampler.")]
        public TextureWrapMode WrapS { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies wrapping parameters for the row coordinates of the texture sampler.")]
        public TextureWrapMode WrapT { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter { get; set; }

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
