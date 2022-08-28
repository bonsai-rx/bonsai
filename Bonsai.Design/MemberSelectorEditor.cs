using System;
using System.Linq;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Dag;
using System.Linq.Expressions;

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
            : this(expression => expression.Type.GetGenericArguments()[0], allowMultiSelection)
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
            : base(DataSource.Input, typeof(void))
        {
            getType = typeSelector ?? throw new ArgumentNullException(nameof(typeSelector));
            isMultiMemberSelector = allowMultiSelection;
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

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && workflowBuilder != null && editorService != null)
            {
                var mapping = GetPropertyMapping(context);
                var source = mapping != null
                    ? GetPropertyMappingBuilder(mapping, provider)
                    : GetDataSource(context, provider);

                if (source != null)
                {
                    var selector = value as string ?? string.Empty;
                    var expression = workflowBuilder.Workflow.Build(source);
                    var expressionType = getType(expression);
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
            }

            return base.EditValue(context, provider, value);
        }
    }
}
