using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Dag;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Bonsai.Design
{
    public class MemberSelectorEditor : UITypeEditor
    {
        bool allowMultiSelection;
        Func<Expression, Type> typeSelector;

        public MemberSelectorEditor()
            : this(false)
        {
        }

        public MemberSelectorEditor(bool allowMultiSelection)
            : this(expression => expression.Type.GetGenericArguments()[0], allowMultiSelection)
        {
        }

        public MemberSelectorEditor(Func<Expression, Type> typeSelector, bool allowMultiSelection)
        {
            if (typeSelector == null)
            {
                throw new ArgumentNullException("typeSelector");
            }

            this.typeSelector = typeSelector;
            this.allowMultiSelection = allowMultiSelection;
        }

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

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var selector = value as string ?? string.Empty;
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var workflowBuilder = (WorkflowBuilder)provider.GetService(typeof(WorkflowBuilder));
                if (workflowBuilder == null) return base.EditValue(context, provider, value);

                var nodeBuilderGraph = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
                if (nodeBuilderGraph == null) return base.EditValue(context, provider, value);

                var workflow = workflowBuilder.Workflow;
                Node<ExpressionBuilder, ExpressionBuilderArgument> builderNode;
                var mapping = GetPropertyMapping(context);
                if (mapping != null)
                {
                    builderNode = GetPropertyMappingBuilderNode(mapping, nodeBuilderGraph, out nodeBuilderGraph);
                }
                else builderNode = (from node in nodeBuilderGraph
                                    let builder = node.Value
                                    where ExpressionBuilder.GetWorkflowElement(builder) == context.Instance
                                    select node).SingleOrDefault();

                if (builderNode == null) return base.EditValue(context, provider, value);
                var predecessor = nodeBuilderGraph.Predecessors(builderNode)
                                                  .Where(node => !node.Value.IsBuildDependency())
                                                  .SingleOrDefault();

                if (predecessor != null)
                {
                    var expression = workflow.Build(predecessor.Value);
                    var expressionType = typeSelector(expression);
                    if (expressionType == null) return base.EditValue(context, provider, value);
                    using (var editorDialog = allowMultiSelection ?
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
