using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Resources
{
    public class ResourceNameConverter : StringConverter
    {
        readonly Type targetType;

        protected ResourceNameConverter(Type type)
        {
            targetType = type;
        }

        protected virtual bool IsResourceSupported(IResourceConfiguration resource)
        {
            return resource.Type == targetType;
        }

        static bool IsGroup(IWorkflowExpressionBuilder builder)
        {
            return builder is IncludeWorkflowBuilder || builder is GroupWorkflowBuilder;
        }

        static IEnumerable<ExpressionBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                var element = ExpressionBuilder.Unwrap(node.Value);
                yield return element;

                var workflowBuilder = element as IWorkflowExpressionBuilder;
                if (IsGroup(workflowBuilder))
                {
                    var workflow = workflowBuilder.Workflow;
                    if (workflow == null) continue;
                    foreach (var groupElement in SelectContextElements(workflow))
                    {
                        yield return groupElement;
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
                var groupBuilder = element as IWorkflowExpressionBuilder;
                if (IsGroup(groupBuilder) && groupBuilder.Workflow == target) return true;

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

        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            if (context != null)
            {
                var workflowBuilder = (WorkflowBuilder)context.GetService(typeof(WorkflowBuilder));
                var nodeBuilderGraph = (ExpressionBuilderGraph)context.GetService(typeof(ExpressionBuilderGraph));
                if (workflowBuilder != null && nodeBuilderGraph != null)
                {
                    var callContext = new Stack<ExpressionBuilderGraph>();
                    if (GetCallContext(workflowBuilder.Workflow, nodeBuilderGraph, callContext))
                    {
                        var names = (from level in callContext
                                     from element in SelectContextElements(level)
                                     let loader = ExpressionBuilder.GetWorkflowElement(element) as ResourceLoader
                                     where loader != null
                                     from resource in loader.GetResources().Where(IsResourceSupported)
                                     select resource.Name)
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
