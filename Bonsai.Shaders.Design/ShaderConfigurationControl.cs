using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Design
{
    public class ShaderConfigurationControl : ConfigurationControl
    {
        public ShaderConfigurationControl()
        {
            //TODO: Extend configuration control to allow updating button text
            var button = Controls.Find("configurationManagerButton", true);
            if (button.Length > 0)
            {
                button[0].Text = "Manage Shaders";
            }
        }

        protected override IEnumerable<string> GetConfigurationNames()
        {
            return ShaderManager.LoadConfiguration().Select(configuration => configuration.Name);
        }

        protected override object LoadConfiguration()
        {
            return ShaderManager.LoadConfiguration();
        }

        protected override void SaveConfiguration(object configuration)
        {
            var shaderConfiguration = configuration as ShaderConfigurationCollection;
            if (shaderConfiguration == null)
            {
                throw new ArgumentNullException("configuration");
            }

            ShaderManager.SaveConfiguration(shaderConfiguration);
        }

        protected override CollectionEditor CreateConfigurationEditor(Type type)
        {
            return new ShaderConfigurationCollectionEditor(type);
        }

        class ShaderConfigurationCollectionEditor : DescriptiveCollectionEditor
        {
            public ShaderConfigurationCollectionEditor(Type type)
                : base(type)
            {
            }
        }
    }
}
