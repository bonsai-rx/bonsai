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

        static IEnumerable<InspectBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                var inspectBuilder = node.Value as InspectBuilder;
                if (inspectBuilder == null) continue;
                yield return inspectBuilder;

                var groupBuilder = inspectBuilder.Builder as IGroupWorkflowBuilder;
                if (groupBuilder != null)
                {
                    var workflow = groupBuilder.Workflow;
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

            foreach (var inspectBuilder in SelectContextElements(source))
            {
                var element = inspectBuilder.Builder;
                var groupBuilder = element as IGroupWorkflowBuilder;
                if (groupBuilder != null && groupBuilder.Workflow == target) return true;

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
                    Type targetType;
                    var componentType = context.PropertyDescriptor.ComponentType;
                    var multicast = componentType == typeof(MulticastSubjectBuilder);
                    if (multicast)
                    {
                        var subjectTargetAttribute = (SubjectTargetAttribute)context.PropertyDescriptor.Attributes[typeof(SubjectTargetAttribute)];
                        targetType = subjectTargetAttribute.TargetType;
                    }
                    else targetType = componentType.IsGenericType ? componentType.GetGenericArguments()[0] : null;

                    var callContext = new Stack<ExpressionBuilderGraph>();
                    if (GetCallContext(workflowBuilder.Workflow, workflow, callContext))
                    {
                        var names = (from level in callContext
                                     from inspectBuilder in SelectContextElements(level)
                                     let subjectBuilder = inspectBuilder.Builder as SubjectBuilder
                                     where subjectBuilder != null && !string.IsNullOrEmpty(subjectBuilder.Name) &&
                                           (targetType == null ||
                                           !multicast && targetType.IsAssignableFrom(inspectBuilder.ObservableType) ||
                                            multicast && ExpressionBuilder.HasConversion(inspectBuilder.ObservableType, targetType))
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
