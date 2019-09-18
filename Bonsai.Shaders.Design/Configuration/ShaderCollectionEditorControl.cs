using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration.Design
{
    [Obsolete]
    class ShaderCollectionEditorControl : CollectionEditorControl
    {
        Lazy<XmlSerializer> textureCollectionSerializer;

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            textureCollectionSerializer = new Lazy<XmlSerializer>(() =>
                new XmlSerializer(typeof(List<TextureConfiguration>)));
        }

        protected override bool DeserializeItems(XmlReader reader)
        {
            var result = base.DeserializeItems(reader);
            if (!result)
            {
                var serializer = textureCollectionSerializer.Value;
                if (serializer.CanDeserialize(reader))
                {
                    var items = (List<TextureConfiguration>)serializer.Deserialize(reader);
                    foreach (var item in items)
                    {
                        var shader = new MaterialConfiguration();
                        shader.Name = item.Name;
                        shader.BufferBindings.Add(new TextureBindingConfiguration
                        {
                            Name = "tex",
                            TextureName = item.Name
                        });
                        AddItem(shader);
                    }

                    return true;
                }
            }

            return result;
        }
    }
}
