using Bonsai.Shaders.Configuration;
using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reactive.Linq;
using System.Xml.Serialization;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that renders all currently stored draw commands
    /// to one of the cubemap textures. Each pass renders one face of the
    /// cubemap in the order +X, -X, +Y, -Y, +Z, -Z.
    /// </summary>
    [Description("Renders all currently stored draw commands to one of the cubemap textures. Each pass renders one face of the cubemap in the order +X, -X, +Y, -Y, +Z, -Z.")]
    public class RenderCubemap : Combinator<Texture>
    {
        readonly StateConfigurationCollection renderState = new StateConfigurationCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="RenderCubemap"/> class.
        /// </summary>
        public RenderCubemap()
        {
            ClearColor = Color.Black;
            ClearMask = ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit;
            MinFilter = TextureMinFilter.Linear;
            MagFilter = TextureMagFilter.Linear;
            InternalFormat = PixelInternalFormat.Rgb;
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
        /// Gets or sets the texture size for each of the cubemap faces. If no
        /// value is specified, the size of the shader window in pixels is used.
        /// </summary>
        [Category("TextureParameter")]
        [Description("The texture size for each of the cubemap faces. If no value is specified, the size of the shader window in pixels is used.")]
        public int? FaceSize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the internal pixel format of the cubemap.
        /// </summary>
        [Category("TextureParameter")]
        [Description("Specifies the internal pixel format of the cubemap.")]
        public PixelInternalFormat InternalFormat { get; set; }

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

        /// <summary>
        /// Renders all currently stored draw commands to one of the cubemap textures
        /// whenever an observable sequence emits a notification. Each pass renders
        /// one face of the cubemap in the order +X, -X, +Y, -Y, +Z, -Z.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence of notifications used to render each of the cubemap faces.
        /// </param>
        /// <returns>
        /// A sequence returning the <see cref="Texture"/> object representing the
        /// cubemap texture, whenever all six faces of the cubemap have been updated.
        /// </returns>
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
