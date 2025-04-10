using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace Bonsai.Resources
{
    /// <summary>
    /// Provides a type converter to convert a resource name to and from other representations.
    /// It also provides a mechanism to find existing resources declared in the workflow.
    /// </summary>
    public class ResourceNameConverter : StringConverter
    {
        readonly Type targetType;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResourceNameConverter"/> class
        /// for the specified type.
        /// </summary>
        /// <param name="type">The type of resources supported by this converter.</param>
        protected ResourceNameConverter(Type type)
        {
            targetType = type;
        }

        /// <summary>
        /// Returns a value indicating whether the specified resource is supported
        /// by this converter.
        /// </summary>
        /// <param name="resource">The resource to be tested.</param>
        /// <returns>
        /// <see langword="true"/> if the specified resource is supported;
        /// <see langword="false"/> otherwise.
        /// </returns>
        protected virtual bool IsResourceSupported(IResourceConfiguration resource)
        {
            return resource.Type == targetType;
        }

        static bool IsGroup(IWorkflowExpressionBuilder builder)
        {
            return builder is GroupWorkflowBuilder || builder is IncludeWorkflowBuilder;
        }

        static IEnumerable<ExpressionBuilder> SelectContextElements(ExpressionBuilderGraph source)
        {
            foreach (var node in source)
            {
                var element = ExpressionBuilder.Unwrap(node.Value);
                if (element is DisableBuilder) continue;
                yield return element;

                if (element is IWorkflowExpressionBuilder workflowBuilder && IsGroup(workflowBuilder))
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
                if (element is IWorkflowExpressionBuilder groupBuilder && IsGroup(groupBuilder))
                {
                    if (groupBuilder.Workflow == target)
                        return true;
                }
                else if (element is WorkflowExpressionBuilder workflowBuilder)
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

        /// <inheritdoc/>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of resource names which are available in the call context
        /// of this type converter request.
        /// </summary>
        /// <returns>
        /// A <see cref="TypeConverter.StandardValuesCollection"/> containing the set of
        /// available resources. Only resources for which <see cref="IsResourceSupported(IResourceConfiguration)"/>
        /// returns <see langword="true"/> will be included.
        /// </returns>
        /// <inheritdoc/>
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
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
