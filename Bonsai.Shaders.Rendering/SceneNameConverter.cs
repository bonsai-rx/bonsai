using Bonsai.Resources;

namespace Bonsai.Shaders.Rendering
{
    class SceneNameConverter : ResourceNameConverter
    {
        public SceneNameConverter()
            : base(typeof(ISceneRenderer))
        {
        }
    }
}
