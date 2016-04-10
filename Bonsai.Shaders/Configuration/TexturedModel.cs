using OpenTK.Graphics.OpenGL4;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration
{
    public class TexturedModel : MeshConfiguration
    {
        [Editor("Bonsai.Design.OpenFileNameEditor, Bonsai.Design", typeof(UITypeEditor))]
        [FileNameFilter("OBJ Files (*.obj)|*.obj")]
        [Description("The name of the model file.")]
        public string FileName { get; set; }

        public override Mesh CreateResource()
        {
            var mesh = base.CreateResource();
            ObjReader.ReadObject(mesh, FileName);
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
