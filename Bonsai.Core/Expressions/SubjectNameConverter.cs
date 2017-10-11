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

        static IEnumerable<ExpressionBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                var element = ExpressionBuilder.Unwrap(node.Value);
                yield return element;

                var includeBuilder = element as IncludeWorkflowBuilder;
                if (includeBuilder != null)
                {
                    var workflow = includeBuilder.Workflow;
                    if (workflow == null) continue;
                    foreach (var includedElement in SelectContextElements(workflow))
                    {
                        yield return includedElement;
                    }
                }
            }
        }

        static bool GetCallContext(ExpressionBuilderGraph source, ExpressionBuilderGraph target, Stack<ExpressionBuilderGraph> context)
        {
            context.Push(source);
            if (source == target)
            {
                return true;
            }

            foreach (var element in SelectContextElements(source))
            {
                var includeBuilder = element as IncludeWorkflowBuilder;
                if (includeBuilder != null && includeBuilder.Workflow == target) return true;

                var workflowBuilder = element as WorkflowExpressionBuilder;
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
                                     from element in SelectContextElements(level)
                                     let subjectBuilder = element as SubjectBuilder
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
