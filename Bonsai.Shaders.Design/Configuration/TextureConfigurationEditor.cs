using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class TextureConfigurationEditor : ShaderConfigurationEditor
    {
        protected override ShaderConfigurationControl CreateEditorControl(IServiceProvider provider)
        {
            return new TextureConfigurationControl(provider);
        }

        class TextureConfigurationControl : ShaderConfigurationControl
        {
            public TextureConfigurationControl(IServiceProvider provider)
                : base(provider)
            {
                Text = "Manage Textures";
            }

            protected override IEnumerable<string> GetConfigurationNames()
            {
                return ShaderManager.LoadConfiguration().Textures.Select(configuration => configuration.Name);
            }

            protected override UITypeEditor CreateConfigurationEditor(Type type)
            {
                return new ShaderWindowEditor
                {
                    SelectedPage = ShaderConfigurationEditorPage.Textures
                };
            }
        }
    }
}
