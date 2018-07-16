using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Returns the sequence of all shader texture names.")]
    public class EnumerateTextures : Source<string>
    {
        public override IObservable<string> Generate()
        {
            return ShaderManager.WindowSource.FirstAsync().SelectMany(window => window.Textures.Keys);
        }
    }
}
