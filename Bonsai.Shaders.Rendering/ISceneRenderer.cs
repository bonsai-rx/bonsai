using System;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Provides common functionality for rendering a scene graph.
    /// </summary>
    public interface ISceneRenderer : IDisposable
    {
        /// <summary>
        /// Gets the root node of the scene graph.
        /// </summary>
        SceneNode RootNode { get; }

        /// <summary>
        /// Draws all nodes in the scene graph.
        /// </summary>
        void Draw();
    }
}
