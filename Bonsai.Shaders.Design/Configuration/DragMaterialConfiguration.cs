using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class DragMaterialConfiguration : DragResourceConfiguration
    {
        public DragMaterialConfiguration(CollectionEditorControl editor)
            : base(editor)
        {
        }

        protected override bool IsResourceAllowed(string fileName)
        {
            return Path.GetExtension(fileName) == ".mtl";
        }

        protected override object CreateResourceConfiguration(string fileName)
        {
            var material = new MaterialConfiguration();
            material.Name = Path.GetFileNameWithoutExtension(fileName);
            MtlReader.ReadMaterial(material, fileName);
            return material;
        }
    }
}
