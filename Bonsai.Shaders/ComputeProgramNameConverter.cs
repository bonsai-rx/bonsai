using Bonsai.Resources;
using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
