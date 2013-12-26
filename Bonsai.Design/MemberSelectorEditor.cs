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
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
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
                var builderNode = (from node in nodeBuilderGraph
                                   let builder = node.Value
                                   where ExpressionBuilder.GetWorkflowElement(builder) == context.Instance
                                   select node).SingleOrDefault();

                if (builderNode == null) return base.EditValue(context, provider, value);
                using (var editorDialog = new MemberSelectorEditorDialog())
                {
                    var predecessorEdges = nodeBuilderGraph.PredecessorEdges(builderNode)
                                                           .OrderBy(edge => edge.Item2.Label.Index)
                                                           .ToArray();

                    foreach (var predecessor in predecessorEdges)
                    {
                        var expression = workflow.Build(predecessor.Item1.Value);
                        var expressionType = expression.Type.GetGenericArguments()[0];
                        var label = predecessorEdges.Length > 1 ? predecessor.Item2.Label.Name : ExpressionBuilderArgument.Source;
                        editorDialog.AddMember(label, expressionType);
                    }

                    editorDialog.Selector = selector;
                    if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                    {
                        return editorDialog.Selector;
                    }
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
