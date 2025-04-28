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

        public WorkflowMeter(ExpressionBuilderGraph workflow, WorkflowWatchMap watchMap)
        {
            counters = GetElements(workflow, watchMap).ToDictionary(
                inspectBuilder => (ExpressionBuilder)inspectBuilder,
                inspectBuilder => new WorkflowElementCounter(inspectBuilder));
        }

        public IReadOnlyDictionary<ExpressionBuilder, WorkflowElementCounter> Counters => counters;

        static IEnumerable<InspectBuilder> GetElements(ExpressionBuilderGraph workflow, WorkflowWatchMap watchMap)
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
                    if (inspectBuilder.PublishNotifications &&
                        inspectBuilder.Watch is not null &&
                        watchMap.Contains(inspectBuilder))
                    {
                        yield return inspectBuilder;

                        if (inspectBuilder.Builder is IWorkflowExpressionBuilder workflowBuilder &&
                            workflowBuilder.Workflow is not null)
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
