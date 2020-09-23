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
            if (disposing && id > 0)
            {
                GL.DeleteTextures(1, ref id);
                id = 0;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
