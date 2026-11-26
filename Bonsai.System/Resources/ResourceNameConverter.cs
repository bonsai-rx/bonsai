using Bonsai.Expressions;
using System;
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

        /// <inheritdoc/>
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return true;
        }

        /// <summary>
        /// Returns a collection of resource names which can be referred to from the workflow
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
                    var names = (from node in workflowBuilder.Workflow.LookupNodes(nodeBuilderGraph)
                                 let loader = ExpressionBuilder.GetWorkflowElement(node.Value) as ResourceLoader
                                 where loader != null
                                 from resource in loader.GetResources().Where(IsResourceSupported)
                                 select resource.Name)
                                 .Distinct()
                                 .ToList();
                    return new StandardValuesCollection(names);
                }
            }

            return base.GetStandardValues(context);
        }
    }
}
