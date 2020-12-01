using Bonsai.Resources;
using System.ComponentModel;

namespace Bonsai.Shaders.Rendering
{
    public class SceneConfiguration
    {
        [Description("The name of the resource.")]
        public string Name { get; set; }

        [TypeConverter(typeof(ResourceFileNameConverter))]
        [Editor(DesignTypes.OpenFileNameEditor, DesignTypes.UITypeEditor)]
        [FileNameFilter("Blender Files (*.blend)|*.blend|OBJ Files (*.obj)|*.obj|All Files|*.*")]
        [Description("The name of the scene file.")]
        public string FileName { get; set; }

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
