using OpenTK;

namespace Bonsai.Shaders
{
    /// <summary>
    /// Represents a point of view from which to render a 3D scene.
    /// </summary>
    public class Camera
    {
        /// <summary>
        /// The view matrix representing a transformation from world coordinates
        /// into eye space coordinates.
        /// </summary>
        public Matrix4 ViewMatrix;

        /// <summary>
        /// The projection matrix representing a transformation from eye space
        /// coordinates into clip space coordinates.
        /// </summary>
        public Matrix4 ProjectionMatrix;

        /// <summary>
        /// Initializes a new instance of the <see cref="Camera"/> class using
        /// the specified view matrix and projection matrix.
        /// </summary>
        /// <param name="view">
        /// The view matrix representing how to transform world coordinates
        /// into eye space coordinates depending on the position and orientation
        /// of the camera.
        /// </param>
        /// <param name="projection">
        /// The projection matrix representing how to transform eye space
        /// coordinates into clip space coordinates.
        /// </param>
        public Camera(Matrix4 view, Matrix4 projection)
        {
            ViewMatrix = view;
            ProjectionMatrix = projection;
        }
    }
}
