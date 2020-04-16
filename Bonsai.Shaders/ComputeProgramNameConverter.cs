using Bonsai.Resources;
using Bonsai.Shaders.Configuration;

namespace Bonsai.Shaders
{
    class ComputeProgramNameConverter : ShaderNameConverter
    {
        protected override bool IsResourceSupported(IResourceConfiguration resource)
        {
            return resource is ComputeProgramConfiguration;
        }
    }
}
