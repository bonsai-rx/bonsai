using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents an array of texture objects.
    /// </summary>
    public class TextureArray : IDisposable, ITextureArray
    {
        readonly int[] textures;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureArray"/> class
        /// with the specified number of texture objects.
        /// </summary>
        /// <param name="length">
        /// The total number of texture objects in the array.
        /// </param>
        public TextureArray(int length)
        {
            textures = new int[length];
            GL.GenTextures(textures.Length, textures);
        }

        internal TextureArray(int[] textures)
        {
            this.textures = textures;
        }

        /// <summary>
        /// Gets the texture object at the specified index.
        /// </summary>
        /// <param name="index">
        /// The zero-based index of the texture object to get.
        /// </param>
        /// <returns>
        /// The handle to the texture object at the specified index.
        /// </returns>
        public int this[int index]
        {
            get { return textures[index]; }
        }

        /// <summary>
        /// Gets the total number of texture objects in the array.
        /// </summary>
        public int Length
        {
            get { return textures.Length; }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="TextureArray"/> class.
        /// </summary>
        public void Dispose()
        {
            GL.DeleteTextures(textures.Length, textures);
        }

        /// <summary>
        /// Returns an enumerator that iterates through all texture objects in
        /// the array.
        /// </summary>
        /// <returns>
        /// An enumerator that can be used to iterate through the texture objects
        /// in the array.
        /// </returns>
        public IEnumerator<int> GetEnumerator()
        {
            for (int i = 0; i < textures.Length; i++)
            {
                yield return textures[i];
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
