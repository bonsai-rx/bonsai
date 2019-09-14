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
    static class ConfigurationHelper
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
                olderVersion = reader.ReadToDescendant("ShaderConfiguration") &&
                               reader.AttributeCount == 0;
            }

            if (olderVersion)
            {
                result = MessageBox.Show(
                    Resources.ConfigurationFileUpgrade_Message,
                    Resources.ConfigurationFileUpgrade_Caption,
                    MessageBoxButtons.YesNoCancel, MessageBoxIcon.Warning);
                if (result == DialogResult.Cancel) return null;

                var overrides = new XmlAttributeOverrides();
                var shaderAttributes = new XmlAttributes();
                var materialElement = new XmlArrayItemAttribute();
                materialElement.ElementName = "ShaderConfiguration";
                materialElement.Type = typeof(MaterialConfiguration);
                shaderAttributes.XmlArrayItems.Add(materialElement);
                overrides.Add(typeof(ShaderWindowSettings), "Shaders", shaderAttributes);

                var bufferBindingAttributes = new XmlAttributes();
                var textureBindingElement = new XmlArrayItemAttribute();
                textureBindingElement.ElementName = "TextureBindingConfiguration";
                textureBindingElement.Type = typeof(TextureBindingConfiguration);
                bufferBindingAttributes.XmlArray = new XmlArrayAttribute("TextureBindings");
                bufferBindingAttributes.XmlArrayItems.Add(textureBindingElement);
                overrides.Add(typeof(ShaderConfiguration), "BufferBindings", bufferBindingAttributes);

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
            try { return ShaderManager.LoadConfiguration(); }
            catch
            {
                result = DialogResult.Cancel;
                throw;
            }
        }
    }
}
