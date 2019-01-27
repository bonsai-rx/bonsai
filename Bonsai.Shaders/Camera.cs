using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    public class Camera
    {
        public Matrix4 ViewMatrix;
        public Matrix4 ProjectionMatrix;

        public Camera(Matrix4 view, Matrix4 projection)
        {
            ViewMatrix = view;
            ProjectionMatrix = projection;
        }
    }
}
