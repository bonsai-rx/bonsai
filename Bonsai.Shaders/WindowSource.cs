using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders
{
    [Description("Gets a handle to the active shader window.")]
    public class WindowSource : Source<ShaderWindow>
    {
        public override IObservable<ShaderWindow> Generate()
        {
            return ShaderManager.WindowSource.FirstAsync();
        }
    }
}
