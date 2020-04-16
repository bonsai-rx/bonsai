using OpenTK;

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
