using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Serialization;

namespace Bonsai.Resources.Design
{
    public class CollectionEditor : UITypeEditor
    {
        Type[] newItemTypes;
        Type collectionItemType;
        readonly Type collectionType;

        public CollectionEditor(Type type)
        {
            collectionType = type;
        }

        protected Type CollectionType
        {
            get { return collectionType; }
        }

        protected Type CollectionItemType
        {
            get
            {
                if (collectionItemType == null)
                {
                    collectionItemType = CreateCollectionItemType();
                }

                return collectionItemType;
            }
        }

        protected Type[] NewItemTypes
        {
            get
            {
                if (newItemTypes == null)
                {
                    newItemTypes = CreateNewItemTypes();
                }

                return newItemTypes;
            }
        }

        protected virtual Type CreateCollectionItemType()
        {
            var defaultMember = TypeDescriptor.GetReflectionType(collectionType)
                .GetDefaultMembers()
                .OfType<PropertyInfo>()
                .FirstOrDefault();
            return defaultMember != null ? defaultMember.PropertyType : typeof(object);
        }

        protected virtual Type[] CreateNewItemTypes()
        {
            var itemType = CollectionItemType;
            var newItemTypes = new List<Type>();
            if (!itemType.IsAbstract) newItemTypes.Add(itemType);
            newItemTypes.AddRange(itemType.GetCustomAttributes<XmlIncludeAttribute>().Select(attribute => attribute.Type));
            return newItemTypes.ToArray();
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
                    collectionForm.NewItemTypes = NewItemTypes;
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
