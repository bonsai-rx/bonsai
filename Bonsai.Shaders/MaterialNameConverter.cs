using Bonsai.Resources;
using Bonsai.Shaders.Configuration;

namespace Bonsai.Shaders
{
    class MaterialNameConverter : ShaderNameConverter
    {
        protected override bool IsResourceSupported(IResourceConfiguration resource)
        {
            return resource is MaterialConfiguration;
        }
    }
}
