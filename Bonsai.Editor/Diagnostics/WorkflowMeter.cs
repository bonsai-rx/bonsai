using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Dag;
using Bonsai.Expressions;

namespace Bonsai.Editor.Diagnostics
{
    internal class WorkflowMeter : IDisposable
    {
        readonly Dictionary<ExpressionBuilder, WorkflowElementCounter> counters;

        public WorkflowMeter(ExpressionBuilderGraph workflow)
        {
            counters = GetElements(workflow).ToDictionary(
                inspectBuilder => (ExpressionBuilder)inspectBuilder,
                inspectBuilder => new WorkflowElementCounter(inspectBuilder));
        }

        public IReadOnlyDictionary<ExpressionBuilder, WorkflowElementCounter> Counters => counters;

        static IEnumerable<InspectBuilder> GetElements(ExpressionBuilderGraph workflow)
        {
            var stack = new Stack<IEnumerator<Node<ExpressionBuilder, ExpressionBuilderArgument>>>();
            stack.Push(workflow.GetEnumerator());

            while (stack.Count > 0)
            {
                var nodeEnumerator = stack.Peek();
                while (true)
                {
                    if (!nodeEnumerator.MoveNext())
                    {
                        stack.Pop();
                        break;
                    }

                    var inspectBuilder = (InspectBuilder)nodeEnumerator.Current.Value;
                    if (inspectBuilder.Output != null)
                    {
                        yield return inspectBuilder;

                        if (inspectBuilder.Builder is IWorkflowExpressionBuilder workflowBuilder &&
                            workflowBuilder.Workflow != null)
                        {
                            stack.Push(workflowBuilder.Workflow.GetEnumerator());
                            break;
                        }
                    }
                }
            }
        }

        public void Dispose()
        {
            foreach (var counter in counters.Values)
            {
                counter.Dispose();
            }
            counters.Clear();
        }
    }
}
