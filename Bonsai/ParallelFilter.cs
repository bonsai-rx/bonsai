using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.ComponentModel;

namespace Bonsai
{
    public class ParallelFilter<T> : Filter<T, T>, IWorkflowContainer
    {
        Action<T> processingChain;
        readonly WorkflowElementCollection components;

        public ParallelFilter()
        {
            components = new WorkflowElementCollection();
        }

        [Browsable(false)]
        public WorkflowElementCollection Components
        {
            get { return components; }
        }

        public override T Process(T input)
        {
            processingChain(input);
            return input;
        }

        public override void Load(WorkflowContext context)
        {
            Expression output = null;
            ParameterExpression input = null;
            output = input = Expression.Parameter(typeof(T));

            foreach (var component in components)
            {
                // Load the workflow element
                component.Load(context);

                // Add another processing step to the workflow expression
                var instance = Expression.Constant(component);
                var method = component.GetType().GetMethod("Process");
                output = Expression.Call(instance, method, output);
            }

            // Store the compiled processing chain
            processingChain = (Action<T>)Expression.Lambda(output, input).Compile();
            base.Load(context);
        }

        public override void Unload(WorkflowContext context)
        {
            processingChain = null;

            // Unload the workflow elements in reverse order
            foreach (var component in components.Reverse())
            {
                component.Unload(context);
            }
            base.Unload(context);
        }
    }
}
