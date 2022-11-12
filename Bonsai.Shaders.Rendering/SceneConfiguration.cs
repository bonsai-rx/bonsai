using Bonsai.Resources;
using System.ComponentModel;

namespace Bonsai.Shaders.Rendering
{
    /// <summary>
    /// Provides configuration information for scene resources.
    /// </summary>
    public class SceneConfiguration
    {
        /// <summary>
        /// Gets or sets the name of the scene.
        /// </summary>
        [Description("The name of the scene.")]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the file from which to load the scene.
        /// </summary>
        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor(DesignTypes.OpenFileNameEditor, DesignTypes.UITypeEditor)]
        [FileNameFilter("COLLADA Files (*.dae)|*.dae|Blender Files (*.blend)|*.blend|FBX Files (*.fbx)|*.fbx|OBJ Files (*.obj)|*.obj|STL Files (*.stl)|*.stl|All Files|*.*")]
        [Description("The name of the file from which to load the scene.")]
        public string FileName { get; set; }

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
