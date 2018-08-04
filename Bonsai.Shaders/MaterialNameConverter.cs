using Bonsai.Shaders.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    class MaterialNameConverter : ShaderNameConverter
    {
        public MaterialNameConverter()
            : base(resource => resource is MaterialConfiguration)
        {
        }
    }
}
