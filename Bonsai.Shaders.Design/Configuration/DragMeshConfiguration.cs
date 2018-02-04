using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class DragMeshConfiguration : DragResourceConfiguration
    {
        public DragMeshConfiguration(CollectionEditorControl editor)
            : base(editor)
        {
        }

        protected override bool IsResourceAllowed(string fileName)
        {
            return Path.GetExtension(fileName) == ".obj";
        }

        protected override object CreateResourceConfiguration(string fileName)
        {
            var configuration = new TexturedModel();
            configuration.FileName = PathConvert.GetProjectPath(fileName);
            configuration.Name = Path.GetFileNameWithoutExtension(fileName);
            return configuration;
        }
    }
}
