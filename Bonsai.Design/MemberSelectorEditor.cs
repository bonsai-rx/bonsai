using System;
using System.Linq;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Dag;
using System.Linq.Expressions;
using System.Collections.Generic;
using System.Reflection;


namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for selecting
    /// members of a workflow expression type.
    /// </summary>
    public class MemberSelectorEditor : DataSourceTypeEditor
    {
        readonly bool isMultiMemberSelector;
        readonly Func<Expression, Type> getType;
        readonly bool isDataSourceSelector;

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberSelectorEditor"/> class.
        /// </summary>
        public MemberSelectorEditor()
            : this(false)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberSelectorEditor"/> class
        /// using either a multi- or single-selection dialog.
        /// </summary>
        /// <param name="allowMultiSelection">
        /// Indicates whether the interface allows selecting multiple members.
        /// </param>
        public MemberSelectorEditor(bool allowMultiSelection)
            :this(allowMultiSelection, true)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberSelectorEditor"/> class
        /// using either a multi- or single-selection dialog for either the data
        /// source of the element or the members of the following element
        /// </summary>
        /// <param name="allowMultiSelection">
        /// Indicates whether the interface allows selecting multiple members.
        /// </param>
        /// <param name="fromDataSource">
        /// Indicates wether the editor retrieves the data from the source node or the susccessor
        /// </param>
        public MemberSelectorEditor(bool allowMultiSelection, bool fromDataSource)
            : this(expression => expression.Type.GetGenericArguments()[0], allowMultiSelection, fromDataSource)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MemberSelectorEditor"/> class
        /// using either a multi- or single-selection dialog and the specified method
        /// for selecting the expression type.
        /// </summary>
        /// <param name="typeSelector">
        /// A method for selecting the type from which to select members.
        /// </param>
        /// <param name="allowMultiSelection">
        /// Indicates whether the interface allows selecting multiple members.
        /// </param>
        public MemberSelectorEditor(Func<Expression, Type> typeSelector, bool allowMultiSelection)
            : this(typeSelector, allowMultiSelection, true) 
        {
        }

        private MemberSelectorEditor(Func<Expression, Type> typeSelector, bool allowMultiSelection, bool fromDataSource)
            : base(DataSource.Input, typeof(void))
        {
            getType = typeSelector ?? throw new ArgumentNullException(nameof(typeSelector));
            isMultiMemberSelector = allowMultiSelection;
            this.isDataSourceSelector = fromDataSource;
        }

        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        static PropertyMapping GetPropertyMapping(ITypeDescriptorContext context)
        {
            var mapping = context.Instance as PropertyMapping;
            if (mapping != null) return mapping;

            var multiSelection = context.Instance as object[];
            if (multiSelection != null)
            {
                for (int i = 0; i < multiSelection.Length; i++)
                {
                    mapping = multiSelection[i] as PropertyMapping;
                    if (mapping == null) break;
                }
            }

            return mapping;
        }

        static ExpressionBuilder GetPropertyMappingBuilder(PropertyMapping mapping, IServiceProvider provider)
        {
            var nodeBuilderGraph = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            if (nodeBuilderGraph == null) return null;

            var builderNode = GetPropertyMappingBuilderNode(mapping, nodeBuilderGraph, out nodeBuilderGraph);
            if (builderNode == null) return null;

            return nodeBuilderGraph.Predecessors(builderNode)
                .Where(node => !node.Value.IsBuildDependency())
               .SingleOrDefault()?.Value;
        }

        static IEnumerable<ExpressionBuilder> GetPropertyMappingSuccessors(PropertyMapping mapping, IServiceProvider provider)
        {
            var nodeBuilderGraph = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
            if (nodeBuilderGraph == null) return Enumerable.Empty<ExpressionBuilder>();

            var builderNode = GetPropertyMappingBuilderNode(mapping, nodeBuilderGraph, out nodeBuilderGraph);
            if (builderNode == null) return Enumerable.Empty<ExpressionBuilder>();

            return nodeBuilderGraph.Successors(builderNode)
                    .Where(node => !node.Value.IsBuildDependency())
                   .Select(n => n.Value);
        }

        static Node<ExpressionBuilder, ExpressionBuilderArgument> GetPropertyMappingBuilderNode(
            PropertyMapping mapping,
            ExpressionBuilderGraph nodeBuilderGraph,
            out ExpressionBuilderGraph mappingBuilderGraph)
        {
            foreach (var node in nodeBuilderGraph)
            {
                var builder = ExpressionBuilder.Unwrap(node.Value);
                var mappingBuilder = builder as PropertyMappingBuilder;
                if (mappingBuilder != null && mappingBuilder.PropertyMappings.Contains(mapping))
                {
                    mappingBuilderGraph = nodeBuilderGraph;
                    return node;
                }

                var workflowBuilder = builder as IWorkflowExpressionBuilder;
                if (workflowBuilder != null && workflowBuilder.Workflow != null)
                {
                    var builderNode = GetPropertyMappingBuilderNode(mapping, workflowBuilder.Workflow, out mappingBuilderGraph);
                    if (builderNode != null) return builderNode;
                }
            }

            mappingBuilderGraph = null;
            return null;
        }

        static IEnumerable<MemberInfo> GetWorkflowBuilderProperties(IWorkflowExpressionBuilder workflowBuilder)
        {
              Attribute[] ExternalizableAttributes =
              [
                  ExternalizableAttribute.Default,
                  DesignTimeVisibleAttribute.Yes
              ];

              return TypeDescriptor.GetProperties(workflowBuilder, ExternalizableAttributes).OfType<ExternalizedPropertyDescriptor>().Select(p => new ExternalizedPropertyInfo(p));
        }

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && workflowBuilder != null && editorService != null)
            {
                Type expressionType = null;
                var mapping = GetPropertyMapping(context);
                var selector = value as string ?? string.Empty;
                if (isDataSourceSelector)
                {
                    var source = mapping != null
                        ? GetPropertyMappingBuilder(mapping, provider)
                        : GetDataSource(context, provider);

                    if (source != null)
                    {
                        var expression = workflowBuilder.Workflow.Build(source);
                        expressionType = getType(expression);
                    }
                }
                else
                {
                    var successors = GetPropertyMappingSuccessors(mapping, provider)
                        .Select(ExpressionBuilder.GetWorkflowElement).ToList();
                    if (successors.Count() > 0)
                    {
                        if (successors.Count() > 1)
                        {
                            var properties = successors.Select(succesor =>
                                succesor switch
                                {
                                    IWorkflowExpressionBuilder builder => GetWorkflowBuilderProperties(builder),
                                    _ => succesor.GetType().GetProperties()
                                });

                            HashSet<MemberInfo> propertySet = null;
                            foreach (var group in properties)
                            {
                                if (propertySet == null)
                                {
                                    propertySet = new HashSet<MemberInfo>(group, MemberInfoComparer.Instance);
                                }
                                else propertySet.IntersectWith(group);
                            }

                            expressionType = new MemberCollection(propertySet, "Collection");
                            
                        }
                        else
                        {
                            var singleSuccessor = successors.First();
                            if (singleSuccessor is IWorkflowExpressionBuilder builder)
                            {
                                var properties = GetWorkflowBuilderProperties(builder);
                                expressionType = new MemberCollection(properties, builder.ToString());
                            }
                            else
                            {
                                expressionType = singleSuccessor.GetType();
                            }
                        }
                    }
                }
                if (expressionType == null) return base.EditValue(context, provider, value);
                using (var editorDialog = isMultiMemberSelector ?
                       (IMemberSelectorEditorDialog)
                       new MultiMemberSelectorEditorDialog(expressionType) :
                       new MemberSelectorEditorDialog(expressionType))
                {
                    editorDialog.Selector = selector;
                    if (editorService.ShowDialog((Form)editorDialog) == DialogResult.OK)
                    {
                        return editorDialog.Selector;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }

        class MemberInfoComparer : IEqualityComparer<MemberInfo>
        {
            public static readonly MemberInfoComparer Instance = new MemberInfoComparer();

            public bool Equals(MemberInfo x, MemberInfo y)
            {
                if (x == null) return y == null;
                else return y != null && x.Name == y.Name && GetType(x) == GetType(y);
            }

            Type GetType(MemberInfo info)
            {
                return info switch
                {
                    PropertyInfo propertyInfo => propertyInfo.PropertyType,
                    FieldInfo fieldInfo => fieldInfo.FieldType,
                    IExtendedVisitableMemberInfo extendedVisitableMemberInfo => extendedVisitableMemberInfo.ExtendedVisitableMemberType,
                    _ => throw new NotSupportedException("Invalid Member")
                };
            }

            public int GetHashCode(MemberInfo obj)
            {
                var hash = 313;
                hash = hash * 523 + EqualityComparer<string>.Default.GetHashCode(obj.Name);
                hash = hash * 523 + EqualityComparer<Type>.Default.GetHashCode(GetType(obj));
                return hash;
            }
        }
    }
}
