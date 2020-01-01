using Bonsai.Resources;
using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TexturedModel : MeshConfiguration
    {
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("OBJ Files (*.obj)|*.obj")]
        [Description("The name of the model file.")]
        public string FileName { get; set; }

        public override Mesh CreateResource(ResourceManager resourceManager)
        {
            var mesh = base.CreateResource(resourceManager);
            using (var stream = OpenResource(FileName))
            using (var reader = new StreamReader(stream))
            {
                ObjReader.ReadObject(mesh, reader);
            }
            return mesh;
        }

        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return string.Format("{0} [{1}]", name, fileName);
        }
    }
}
