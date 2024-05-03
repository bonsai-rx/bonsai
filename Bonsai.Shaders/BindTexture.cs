using OpenTK.Graphics.OpenGL4;
using System;
using System.ComponentModel;
using System.Reactive.Linq;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an operator that binds a buffer to the specified texture unit
    /// for each texture or notification in the sequence.
    /// </summary>
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Sink)]
    [Description("Binds a texture buffer to the specified texture unit for each texture or notification in the sequence.")]
    public class BindTexture
    {
        /// <summary>
        /// Gets or sets a value specifying the slot on which to bind the texture.
        /// </summary>
        [Description("Specifies the slot on which to bind the texture.")]
        public TextureUnit TextureSlot { get; set; } = TextureUnit.Texture0;

        /// <summary>
        /// Gets or sets the name of the shader program.
        /// </summary>
        [TypeConverter(typeof(ShaderNameConverter))]
        [Description("The name of the shader program.")]
        public string ShaderName { get; set; }

        /// <summary>
        /// Gets or sets the name of the texture to be bound to the shader.
        /// </summary>
        [TypeConverter(typeof(TextureNameConverter))]
        [Description("The name of the texture to be bound to the shader.")]
        public string TextureName { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the texture target to be bound
        /// to the sampler.
        /// </summary>
        [Description("Specifies the texture target to be bound to the sampler.")]
        public TextureTarget TextureTarget { get; set; } = TextureTarget.Texture2D;

        /// <summary>
        /// Gets or sets the index of the texture to be bound to the shader.
        /// Only applicable to texture array objects.
        /// </summary>
        [Description("The index of the texture to be bound to the shader. Only applicable to texture array objects.")]
        public int? Index { get; set; }

        /// <summary>
        /// Gets a value indicating whether the <see cref="Index"/> property
        /// should be serialized.
        /// </summary>
        [Browsable(false)]
        public bool IndexSpecified
        {
            get { return Index.HasValue; }
        }

        IObservable<TSource> Process<TSource>(IObservable<TSource> source, bool allowUnbind)
        {
            return Observable.Defer(() =>
            {
                var cachedTexture = default(Texture);
                var cachedTextureName = default(string);
                return source.CombineEither(
                    ShaderManager.ReserveShader(ShaderName),
                    (input, shader) =>
                    {
                        if (cachedTextureName != TextureName)
                        {
                            cachedTextureName = TextureName;
                            cachedTexture = !string.IsNullOrEmpty(cachedTextureName)
                                ? shader.Window.ResourceManager.Load<Texture>(cachedTextureName)
                                : null;
                        }

                        var texture = cachedTexture ?? input as Texture;

                        if (texture != null || allowUnbind)
                        {
                            var textureId = texture is null
                                ? 0
                                : Index is int index
                                ? ((TextureSequence)texture).Textures[index]
                                : texture.Id;
                            shader.Update(() =>
                            {
                                GL.ActiveTexture(TextureSlot);
                                GL.BindTexture(TextureTarget, textureId);
                            });
                        }

                        return input;
                    });
            });
        }

        /// <summary>
        /// Binds the specified texture buffer to the specified texture unit for
        /// each notification in an observable sequence.
        /// </summary>
        /// <typeparam name="TSource">
        /// The type of the elements in the <paramref name="source"/> sequence.
        /// </typeparam>
        /// <param name="source">
        /// The sequence containing the notifications indicating when to bind
        /// the texture buffer to the specified texture unit.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of binding the texture buffer to the
        /// specified texture unit.
        /// </returns>
        public IObservable<TSource> Process<TSource>(IObservable<TSource> source)
            => Process(source, allowUnbind: false);

        /// <summary>
        /// Binds each texture buffer in an observable sequence to the specified
        /// texture unit for each notification in an observable sequence.
        /// </summary>
        /// <remarks>
        /// If the <see cref="TextureName"/> property is specified, the corresponding
        /// texture buffer will be used instead of the values in the
        /// <paramref name="source"/> sequence.
        /// </remarks>
        /// <param name="source">
        /// A sequence of <see cref="Texture"/> objects to be bound to the specified
        /// texture unit.
        /// </param>
        /// <returns>
        /// An observable sequence that is identical to the source sequence but where
        /// there is an additional side effect of binding the texture buffer objects
        /// in the sequence to the specified texture unit.
        /// </returns>
        public IObservable<Texture> Process(IObservable<Texture> source)
            => Process(source, allowUnbind: true);
    }
}
