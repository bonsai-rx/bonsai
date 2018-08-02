using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Shaders.Configuration.Design
{
    class CollectionEditor : UITypeEditor
    {
        public CollectionEditor(Type type)
        {
            
        }

        protected virtual Type CreateCollectionItemType()
        {
            return null;
        }

        protected virtual Type[] CreateNewItemTypes()
        {
            return Type.EmptyTypes;
        }

        protected virtual CollectionEditorDialog CreateEditorDialog()
        {
            return new CollectionEditorDialog();
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        protected virtual void SetItems(object editValue, IEnumerable items)
        {
            var collection = editValue as IList;
            if (collection != null)
            {
                collection.Clear();
                foreach (var item in items)
                {
                    collection.Add(item);
                }
            }
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                using (var collectionForm = CreateEditorDialog())
                {
                    var itemType = CreateCollectionItemType();
                    collectionForm.ServiceProvider = provider;
                    collectionForm.CollectionItemType = itemType;
                    collectionForm.NewItemTypes = CreateNewItemTypes();
                    collectionForm.Items = value as IEnumerable;
                    collectionForm.Text = itemType.Name + " " + collectionForm.Text;
                    if (editorService.ShowDialog(collectionForm) == DialogResult.OK)
                    {
                        SetItems(value, collectionForm.Items);
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
