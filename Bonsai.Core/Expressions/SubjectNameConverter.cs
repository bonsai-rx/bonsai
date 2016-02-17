using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Expressions
{
    class SubjectNameConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        bool GetCallContext(ExpressionBuilderGraph source, ExpressionBuilderGraph target, Stack<ExpressionBuilderGraph> context)
        {
            context.Push(source);
            if (source == target)
            {
                return true;
            }

            foreach (var node in source)
            {
                var workflowBuilder = ExpressionBuilder.Unwrap(node.Value) as WorkflowExpressionBuilder;
                if (workflowBuilder != null)
                {
                    if (GetCallContext(workflowBuilder.Workflow, target, context))
                    {
                        return true;
                    }
                }
            }

            context.Pop();
            return false;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflow = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                if (workflow != null && workflowBuilder != null)
                {
                    var callContext = new Stack<ExpressionBuilderGraph>();
                    if (GetCallContext(workflowBuilder.Workflow, workflow, callContext))
                    {
                        var names = (from level in callContext
                                     from node in level
                                     let subjectBuilder = ExpressionBuilder.Unwrap(node.Value) as SubjectBuilder
                                     where subjectBuilder != null
                                     select subjectBuilder.Name)
                                     .Distinct()
                                     .ToList();
                        return new StandardValuesCollection(names);
                    }
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
