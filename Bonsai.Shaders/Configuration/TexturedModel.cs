using Bonsai.Resources;
using System.ComponentModel;
using System.IO;
using System.Xml.Serialization;

namespace Bonsai.Shaders.Configuration
{
    /// <summary>
    /// Provides configuration and loading functionality for textured mesh resources
    /// specified as OBJ files.
    /// </summary>
    [XmlType(Namespace = Constants.XmlNamespace)]
    public class TexturedModel : MeshConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the model OBJ file.
        /// </summary>
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", DesignTypes.UITypeEditor)]
        [FileNameFilter("OBJ Files (*.obj)|*.obj")]
        [Description("The name of the model OBJ file.")]
        public string FileName { get; set; }

        /// <summary>
        /// Creates a new mesh resource using the geometry specified in the OBJ file.
        /// </summary>
        /// <returns>
        /// A new instance of the <see cref="Mesh"/> class storing the geometry
        /// specified in the OBJ file.
        /// </returns>
        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            var name = Name;
            var fileName = FileName;
            var typeName = GetType().Name;
            if (string.IsNullOrEmpty(name)) return typeName;
            else if (string.IsNullOrEmpty(fileName)) return name;
            else return $"{name} [{fileName}]";
        }
    }
}
