using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Xml.Serialization;

namespace Bonsai.Resources.Design
{
    public class CollectionEditor : UITypeEditor
    {
        Type[] newItemTypes;
        Type collectionItemType;

        public CollectionEditor(Type type)
        {
            CollectionType = type ?? throw new ArgumentNullException(nameof(type));
        }

        protected Type CollectionType { get; }

        protected internal Type CollectionItemType
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

        protected internal Type[] NewItemTypes
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
            var defaultMember = TypeDescriptor.GetReflectionType(CollectionType)
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

        protected internal virtual string GetDisplayText(object value)
        {
            if (value == null) return CollectionItemType.ToString();
            return value.ToString();
        }

        protected virtual CollectionEditorDialog CreateEditorDialog()
        {
            return new CollectionEditorDialog(this);
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
                    collectionForm.ServiceProvider = provider;
                    collectionForm.Items = value as IEnumerable;
                    collectionForm.Text = CollectionItemType.Name + " " + collectionForm.Text;
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
