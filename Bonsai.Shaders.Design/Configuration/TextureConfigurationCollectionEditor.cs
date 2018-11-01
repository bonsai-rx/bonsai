using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Shaders.Configuration.Design
{
    class TextureConfigurationCollectionEditor : CollectionEditor
    {
        public TextureConfigurationCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override Type CreateCollectionItemType()
        {
            return typeof(TextureConfiguration);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[] { typeof(Texture2D), typeof(Cubemap), typeof(ImageTexture), typeof(ImageCubemap) };
        }

        protected override CollectionEditorDialog CreateEditorDialog()
        {
            var form = base.CreateEditorDialog();
            form.Tag = new DragTextureConfiguration(form.EditorControl);
            return form;
        }
    }
}
