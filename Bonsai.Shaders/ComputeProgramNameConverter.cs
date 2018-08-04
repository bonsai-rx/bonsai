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
        public ComputeProgramNameConverter()
        {
            targetResource = resource => resource is ComputeProgramConfiguration;
        }
    }
}
