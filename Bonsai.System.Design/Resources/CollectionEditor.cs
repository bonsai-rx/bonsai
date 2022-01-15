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
    /// <summary>
    /// Provides a user interface editor that displays a dialog for editing
    /// a collection of objects.
    /// </summary>
    public class CollectionEditor : UITypeEditor
    {
        Type[] newItemTypes;
        Type collectionItemType;

        /// <summary>
        /// Initializes a new instance of the <see cref="CollectionEditor"/> class
        /// using the specified collection type.
        /// </summary>
        /// <param name="type">
        /// The type of the collection for this editor to edit.
        /// </param>
        public CollectionEditor(Type type)
        {
            CollectionType = type ?? throw new ArgumentNullException(nameof(type));
        }

        /// <summary>
        /// Gets the type of the collection.
        /// </summary>
        protected Type CollectionType { get; }

        /// <summary>
        /// Gets the type of the items in the collection.
        /// </summary>
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

        /// <summary>
        /// Gets the available types of items that can be created for this collection.
        /// </summary>
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

        /// <summary>
        /// Gets the type of the items in this collection.
        /// </summary>
        /// <returns>
        /// The type of of the items in this collection, or <see cref="object"/> if no
        /// <c>Item</c> property can be located on the collection.
        /// </returns>
        protected virtual Type CreateCollectionItemType()
        {
            var defaultMember = TypeDescriptor.GetReflectionType(CollectionType)
                .GetDefaultMembers()
                .OfType<PropertyInfo>()
                .FirstOrDefault();
            return defaultMember != null ? defaultMember.PropertyType : typeof(object);
        }

        /// <summary>
        /// Gets the available types of items that can be created for this collection.
        /// </summary>
        /// <returns>
        /// An array of types that this collection can contain.
        /// </returns>
        protected virtual Type[] CreateNewItemTypes()
        {
            var itemType = CollectionItemType;
            var newItemTypes = new List<Type>();
            if (!itemType.IsAbstract) newItemTypes.Add(itemType);
            newItemTypes.AddRange(itemType.GetCustomAttributes<XmlIncludeAttribute>().Select(attribute => attribute.Type));
            return newItemTypes.ToArray();
        }

        /// <summary>
        /// Retrieves the display text for the specified collection item.
        /// </summary>
        /// <param name="value">
        /// The collection item for which to retrieve display text.
        /// </param>
        /// <returns>
        /// The display text for the specified item value.
        /// </returns>
        protected internal virtual string GetDisplayText(object value)
        {
            if (value == null) return CollectionItemType.ToString();
            return value.ToString();
        }

        /// <summary>
        /// Creates a new dialog to display and edit the current collection.
        /// </summary>
        /// <returns>
        /// A <see cref="CollectionEditorDialog"/> to provide as a user interface for
        /// editing the collection.
        /// </returns>
        protected virtual CollectionEditorDialog CreateEditorDialog()
        {
            return new CollectionEditorDialog(this);
        }

        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <summary>
        /// Sets the specified sequence as the items of the collection.
        /// </summary>
        /// <param name="editValue">The collection to edit.</param>
        /// <param name="items">
        /// A sequence of objects to set as collection items.
        /// </param>
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

        /// <inheritdoc/>
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
