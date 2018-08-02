using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Bonsai.Shaders.Configuration.Design
{
    class MeshConfigurationCollectionEditor : CollectionEditor
    {
        public MeshConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(MeshConfiguration);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(MeshConfiguration), typeof(TexturedQuad), typeof(TexturedModel) };
        }

        protected override CollectionEditorDialog CreateEditorDialog()
        {
            var form = base.CreateEditorDialog();
            form.Tag = new DragMeshConfiguration(form.EditorControl);
            return form;
        }
    }
}
