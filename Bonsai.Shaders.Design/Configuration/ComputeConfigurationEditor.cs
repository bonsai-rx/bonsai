using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class ComputeConfigurationEditor : ShaderConfigurationEditor
    {
        protected override ShaderConfigurationControl CreateEditorControl()
        {
            return new ComputeConfigurationControl();
        }

        class ComputeConfigurationControl : ShaderConfigurationControl
        {
            protected override IEnumerable<string> GetConfigurationNames()
            {
                return ShaderManager.LoadConfiguration().Shaders.Where(configuration => configuration is ComputeConfiguration)
                                                                .Select(configuration => configuration.Name);
            }
        }
    }
}
