using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace Bonsai.Design
{
    class MappingCollectionEditor : DescriptiveCollectionEditor
    {
        static readonly Attribute[] ExternalizableAttributes = new Attribute[]
        {
            ExternalizableAttribute.Default,
            DesignTimeVisibleAttribute.Yes
        };

        public MappingCollectionEditor(Type type)
            : base(type)
        {
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            base.EditValue(context, provider, value);
            var editorService = (IUIService)provider.GetService(typeof(IUIService));
            if (editorService != null) editorService.SetUIDirty();
            return value;
        }

        protected override object[] GetItems(object editValue)
        {
            return base.GetItems(editValue);
        }

        protected override System.Collections.IList GetObjectsFromInstance(object instance)
        {
            if (instance == AllProperties.Instance)
            {
                var workflow = (ExpressionBuilderGraph)Context.GetService(typeof(ExpressionBuilderGraph));
                if (workflow != null)
                {
                    var builderNode = workflow.FirstOrDefault(node =>
                        ExpressionBuilder.GetWorkflowElement(node.Value) == Context.Instance);
                    if (builderNode != null)
                    {
                        var nameProperty = TypeDescriptor.GetProperties(CollectionItemType)["Name"];
                        var properties = from successor in builderNode.Successors
                                         let element = ExpressionBuilder.GetWorkflowElement(successor.Target.Value)
                                         where element != null
                                         select from descriptor in TypeDescriptor.GetProperties(element, ExternalizableAttributes)
                                                                                 .Cast<PropertyDescriptor>()
                                                where descriptor.IsBrowsable && !descriptor.IsReadOnly
                                                select descriptor;
                        HashSet<PropertyDescriptor> propertySet = null;
                        foreach (var group in properties)
                        {
                            if (propertySet == null)
                            {
                                propertySet = new HashSet<PropertyDescriptor>(group, PropertyDescriptorComparer.Instance);
                            }
                            else propertySet.IntersectWith(group);
                        }

                        var collectionItems = new System.Collections.ArrayList();
                        foreach (var property in propertySet)
                        {
                            var mapping = CreateInstance(CollectionItemType);
                            nameProperty.SetValue(mapping, property.Name);
                            collectionItems.Add(mapping);
                        }
                        return collectionItems;
                    }
                }

                base.GetObjectsFromInstance(instance);
            }
            return base.GetObjectsFromInstance(instance);
        }

        protected override object CreateInstance(Type itemType)
        {
            if (itemType == typeof(AllProperties)) return AllProperties.Instance;
            return base.CreateInstance(itemType);
        }

        protected override Type[] CreateNewItemTypes()
        {
            return new[]
            {
                CollectionItemType,
                typeof(AllProperties)
            };
        }

        static class AllProperties
        {
            internal static object Instance = new object();
        }

        class PropertyDescriptorComparer : IEqualityComparer<PropertyDescriptor>
        {
            public static readonly PropertyDescriptorComparer Instance = new PropertyDescriptorComparer();

            public bool Equals(PropertyDescriptor x, PropertyDescriptor y)
            {
                if (x == null) return y == null;
                else return y != null && x.Name == y.Name && x.PropertyType == y.PropertyType;
            }

            public int GetHashCode(PropertyDescriptor obj)
            {
                var hash = 313;
                hash = hash * 523 + EqualityComparer<string>.Default.GetHashCode(obj.Name);
                hash = hash * 523 + EqualityComparer<Type>.Default.GetHashCode(obj.PropertyType);
                return hash;
            }
        }
    }
}
