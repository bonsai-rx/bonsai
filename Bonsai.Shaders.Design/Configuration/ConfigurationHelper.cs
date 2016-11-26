using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration.Design
{
    class ConfigurationHelper
    {
        public static ShaderWindowSettings LoadConfiguration(out DialogResult result)
        {
            if (!File.Exists(ShaderManager.DefaultConfigurationFile))
            {
                result = DialogResult.OK;
                return new ShaderWindowSettings();
            }

            var olderVersion = false;
            using (var reader = XmlReader.Create(ShaderManager.DefaultConfigurationFile))
            {
                olderVersion = reader.ReadToDescendant("Shaders");
            }

            if (olderVersion)
            {
                result = MessageBox.Show(
                    "The current shader configuration file was created with an older version of the shaders module. Would you like to upgrade? Note: you need to upgrade in order to change any settings but this operation is non-reversible.",
                    "Shader Configuration Editor",
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel) return null;

                var overrides = new XmlAttributeOverrides();
                var materialAttributes = new XmlAttributes();
                var materialElement = new XmlArrayItemAttribute();
                materialElement.ElementName = "ShaderConfiguration";
                materialElement.Type = typeof(MaterialConfiguration);
                materialAttributes.XmlArray = new XmlArrayAttribute("Shaders");
                materialAttributes.XmlArrayItems.Add(materialElement);
                overrides.Add(typeof(ShaderWindowSettings), "Materials", materialAttributes);

                ShaderWindowSettings configuration;
                var serializer = new XmlSerializer(typeof(ShaderWindowSettings), overrides);
                using (var reader = XmlReader.Create(ShaderManager.DefaultConfigurationFile))
                {
                    configuration = (ShaderWindowSettings)serializer.Deserialize(reader);
                }

                if (result == DialogResult.Yes) ShaderManager.SaveConfiguration(configuration);
                else return configuration;
            }

            result = DialogResult.OK;
            return ShaderManager.LoadConfiguration();
        }
    }
}
