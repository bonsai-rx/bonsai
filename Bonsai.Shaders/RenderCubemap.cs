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
    [Description("Renders all currently stored draw commands to a cubemap texture. Each pass renders one face of the cubemap in the order +X, -X, +Y, -Y, +Z, -Z.")]
    public class RenderCubemap : Combinator<Texture>
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        public RenderCubemap()
        {
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMagFilter.Linear;
            InternalFormat = PixelInternalFormat.Rgb;
        }

        [Description("Specifies any render states that are required to render the framebuffer.")]
        [Editor("Bonsai.Shaders.Configuration.Design.StateConfigurationCollectionEditor, Bonsai.Shaders.Design", DesignTypes.UITypeEditor)]
        public StateConfigurationCollection RenderState
        {
            get { return renderState; }
        }

        [XmlIgnore]
        [Description("The color used to clear the framebuffer before rendering.")]
        public Color ClearColor { get; set; }

        [Browsable(false)]
        [XmlElement("ClearColor")]
        public string ClearColorHtml
        {
            get { return ColorTranslator.ToHtml(ClearColor); }
            set { ClearColor = ColorTranslator.FromHtml(value); }
        }

        [Description("Specifies which buffers to clear before rendering.")]
        public ClearBufferMask ClearMask { get; set; }

        [Category("TextureParameter")]
        [Description("The optional texture size for each of the cubemap faces.")]
        public int? FaceSize { get; set; }

        [Category("TextureParameter")]
        [Description("The internal pixel format of the cubemap.")]
        public PixelInternalFormat InternalFormat { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies the texture minification filter.")]
        public TextureMinFilter MinFilter { get; set; }

        [Category("TextureParameter")]
        [Description("Specifies the texture magnification filter.")]
        public TextureMagFilter MagFilter { get; set; }

        public override IObservable<Texture> Process<TSource>(IObservable<TSource> source)
        {
            return Observable.Defer(() =>
            {
                var fbo = 0;
                var faceSize = 0;
                var depthRenderbuffer = 0;
                var colorTarget = default(Texture);
                var textureTargetBase = TextureTarget.TextureCubeMapPositiveX;
                var textureIndex = 0;

                return source.CombineEither(
                    ShaderManager.WindowSource.Do(window =>
                    {
                        window.Update(() =>
                        {
                            colorTarget = new Texture();
                            GL.BindTexture(TextureTarget.TextureCubeMap, colorTarget.Id);
                            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge);
                            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge);
                            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
                            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMinFilter, (int)MinFilter);
                            GL.TexParameter(TextureTarget.TextureCubeMap, TextureParameterName.TextureMagFilter, (int)MagFilter);

                            faceSize = FaceSize.GetValueOrDefault(Math.Max(window.Width, window.Height));
                            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeX, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveY, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeY, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                            GL.TexImage2D(TextureTarget.TextureCubeMapPositiveZ, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);
                            GL.TexImage2D(TextureTarget.TextureCubeMapNegativeZ, 0, InternalFormat, faceSize, faceSize, 0, PixelFormat.Bgr, PixelType.UnsignedByte, IntPtr.Zero);

                            GL.GenFramebuffers(1, out fbo);
                            GL.GenRenderbuffers(1, out depthRenderbuffer);
                            GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                            GL.BindRenderbuffer(RenderbufferTarget.Renderbuffer, depthRenderbuffer);
                            GL.RenderbufferStorage(RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent, faceSize, faceSize);
                            GL.FramebufferRenderbuffer(FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment, RenderbufferTarget.Renderbuffer, depthRenderbuffer);
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
                        var textureTarget = textureIndex + textureTargetBase;
                        GL.BindFramebuffer(FramebufferTarget.Framebuffer, fbo);
                        GL.FramebufferTexture2D(FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, textureTarget, colorTarget.Id, 0);
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
                        textureIndex = (textureIndex + 1) % 6;
                        return colorTarget;
                    }).Where(texture => textureIndex == 0).Finally(() =>
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
