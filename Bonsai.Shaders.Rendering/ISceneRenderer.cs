using System;

namespace Bonsai.Shaders.Rendering
{
    public interface ISceneRenderer : IDisposable
    {
        SceneNode RootNode { get; }

        void Draw();
    }
}
