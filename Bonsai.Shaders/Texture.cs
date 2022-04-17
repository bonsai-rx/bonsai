using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents a texture object containing one or more images with the
    /// same image format.
    /// </summary>
    public class Texture : IDisposable
    {
        int id;

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture"/> class.
        /// </summary>
        public Texture()
        {
            GL.GenTextures(1, out id);
        }

        internal Texture(int value)
        {
            id = value;
        }

        /// <summary>
        /// Gets the handle to the texture object.
        /// </summary>
        public int Id
        {
            get { return id; }
            internal set { id = value; }
        }

        internal virtual void Dispose(bool disposing)
        {
            if (disposing && id > 0)
            {
                GL.DeleteTextures(1, ref id);
                id = 0;
            }
        }

        /// <summary>
        /// Releases all resources used by the <see cref="Texture"/> class.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
