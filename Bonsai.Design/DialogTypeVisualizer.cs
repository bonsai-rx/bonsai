using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Bonsai.Design
{
    public abstract class DialogTypeVisualizer
    {
        public abstract void Show(object value);

        public abstract void Load(IServiceProvider provider);

        public abstract void Unload();
    }
}
