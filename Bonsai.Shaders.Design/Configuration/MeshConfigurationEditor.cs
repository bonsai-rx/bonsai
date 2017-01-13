using System;
using System.Collections.Generic;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class MeshConfigurationEditor : ShaderConfigurationEditor
    {
        protected override ShaderConfigurationControl CreateEditorControl(IServiceProvider provider)
        {
            return new MeshConfigurationControl(provider);
        }

        class MeshConfigurationControl : ShaderConfigurationControl
        {
            public MeshConfigurationControl(IServiceProvider provider)
                : base(provider)
            {
                Text = "Manage Meshes";
            }

            protected override IEnumerable<string> GetConfigurationNames()
            {
                return ShaderManager.LoadConfiguration().Meshes.Select(configuration => configuration.Name);
            }

            protected override UITypeEditor CreateConfigurationEditor(Type type)
            {
                return new ShaderWindowEditor
                {
                    SelectedPage = ShaderConfigurationEditorPage.Meshes
                };
            }
        }
    }
}
