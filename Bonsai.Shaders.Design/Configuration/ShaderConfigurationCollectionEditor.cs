using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class ShaderConfigurationCollectionEditor : CollectionEditor
    {
        public ShaderConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(ShaderConfiguration);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(MaterialConfiguration), typeof(ViewportEffectConfiguration), typeof(ComputeProgramConfiguration) };
        }

        protected override CollectionEditorDialog CreateEditorDialog()
        {
            var form = base.CreateEditorDialog();
            form.Tag = new DragMaterialConfiguration(form.EditorControl);
            return form;
        }
    }
}
