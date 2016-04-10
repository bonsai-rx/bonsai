using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class Texture : IDisposable
    {
        int id;

        public Texture()
        {
            GL.GenTextures(1, out id);
        }

        public int Id
        {
            get { return id; }
        }

        public void Dispose()
        {
            GL.DeleteTextures(1, ref id);
        }
    }
}
