using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class TextureBindingConfigurationCollection : KeyedCollection<string, TextureBindingConfiguration>
    {
        protected override string GetKeyForItem(TextureBindingConfiguration item)
        {
            return item.Name;
        }
    }
}
