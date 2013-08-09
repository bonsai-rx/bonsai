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
                var predecessorNode = (from node in nodeBuilderGraph
                                       let builder = node.Value
                                       where ExpressionBuilder.GetWorkflowElement(builder) == context.Instance
                                       select nodeBuilderGraph.Predecessors(node).SingleOrDefault()).SingleOrDefault();

                if (predecessorNode == null) return base.EditValue(context, provider, value);
                var expressionType = workflow.Build(predecessorNode.Value).Type.GetGenericArguments()[0];

                var editorDialog = new MemberSelectorEditorDialog(expressionType, selector);
                if (editorService.ShowDialog(editorDialog) == DialogResult.OK)
                {
                    return string.Join(ExpressionHelper.MemberSeparator, editorDialog.GetMemberChain().ToArray());
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
