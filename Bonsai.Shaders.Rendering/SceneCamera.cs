using OpenTK;
using System;

namespace Bonsai.Shaders.Rendering
{
    class SceneCamera
    {
        readonly Func<Matrix4> getProjectionMatrix;

        internal SceneCamera(Assimp.Camera camera, ShaderWindow window, SceneNode node)
        {
            getProjectionMatrix = () =>
            {
                var aspectRatio = camera.AspectRatio;
                if (aspectRatio == 0)
                {
                    var viewport = window.Viewport;
                    aspectRatio = (viewport.Width * window.Width) / (viewport.Height * window.Height);
                }

                // convert 1/2 horizontal FOV to full vertical FOV
                var fovy = 2 * Math.Atan(Math.Tan(camera.FieldOfview) / aspectRatio);
                return Matrix4.CreatePerspectiveFieldOfView(
                    (float)fovy,
                    aspectRatio,
                    camera.ClipPlaneNear,
                    camera.ClipPlaneFar);
            };
            Name = camera.Name;
            Node = node;
        }

        public string Name { get; private set; }

        public SceneNode Node { get; private set; }

        public Matrix4 ProjectionMatrix => getProjectionMatrix();

        public Matrix4 ViewMatrix
        {
            get { return Node.Transform.Inverted(); }
        }
    }
}
