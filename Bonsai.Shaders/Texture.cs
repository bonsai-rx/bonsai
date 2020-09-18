using OpenTK.Graphics.OpenGL4;
using System;

namespace Bonsai.Shaders
{
    public class Texture : IDisposable
    {
        int id;

        public Texture()
        {
            GL.GenTextures(1, out id);
        }

        internal Texture(int value)
        {
            id = value;
        }

        public int Id
        {
            get { return id; }
            internal set { id = value; }
        }

        internal virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                GL.DeleteTextures(1, ref id);
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
