using Bonsai.Expressions;
using Bonsai.Dag;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Editor
{
    static class UpgradeHelper
    {
        static readonly SemanticVersion DeprecationTarget = SemanticVersion.Parse("2.4.0");
        static readonly SemanticVersion EnumerableUnfoldingVersion = SemanticVersion.Parse("2.3.0");

        internal static bool IsDeprecated(SemanticVersion version)
        {
            return version < DeprecationTarget;
        }

        internal static ExpressionBuilderGraph UpgradeBuilderNodes(ExpressionBuilderGraph workflow)
        {
            return workflow.Convert(builder =>
            {
                var sourceBuilder = builder as SourceBuilder;
                if (sourceBuilder != null)
                {
                    return new CombinatorBuilder
                    {
                        Combinator = sourceBuilder.Generator
                    };
                }

                var property = builder as ExternalizedProperty;
                if (property != null)
                {
                    return new ExternalizedProperty
                    {
                        MemberName = property.MemberName,
                        Name = property.Name
                    };
                }

                return builder;
            });
        }

        internal static void UpgradeEnumerableUnfoldingRules(WorkflowBuilder workflowBuilder, SemanticVersion version)
        {
            if (version < EnumerableUnfoldingVersion)
            {
                var upgradeTargets = GetEnumerableUpgradeTargets(workflowBuilder.Workflow).ToList();
                foreach (var upgradeTarget in upgradeTargets)
                {
                    UpgradeEnumerableInputDependency(workflowBuilder, upgradeTarget);
                }
            }
        }

        struct WorkflowInputDependency
        {
            public ExpressionBuilder Dependency;
            public ExpressionBuilderGraph Target;
        }

        static IEnumerable<WorkflowInputDependency> GetEnumerableUpgradeTargets(ExpressionBuilderGraph workflow)
        {
            foreach (var node in workflow.TopologicalSort())
            {
                var workflowBuilder = node.Value as WorkflowExpressionBuilder;
                if (workflowBuilder != null)
                {
                    foreach (var dependency in GetEnumerableUpgradeTargets(workflowBuilder.Workflow))
                    {
                        yield return dependency;
                    }
                }

                var successor = node.Successors.FirstOrDefault(edge => edge.Label.Index == 0 &&
                                                                       (edge.Target.Value is SelectManyBuilder ||
                                                                        edge.Target.Value is WindowWorkflowBuilder));
                if (successor != null)
                {
                    var inputDependency = new WorkflowInputDependency();
                    inputDependency.Dependency = node.Value;
                    inputDependency.Target = ((WorkflowExpressionBuilder)successor.Target.Value).Workflow;
                    yield return inputDependency;
                }
            }
        }

        static void UpgradeEnumerableInputDependency(WorkflowBuilder builder, WorkflowInputDependency inputDependency)
        {
            var dependency = builder.Workflow.Build(inputDependency.Dependency);
            var sourceType = dependency.Type.GetGenericArguments()[0];
            if (ExpressionHelper.IsEnumerableType(sourceType) && sourceType != typeof(string))
            {
                var workflow = inputDependency.Target;
                var inputNode = workflow.FirstOrDefault(node =>
                {
                    var inputBuilder = ExpressionBuilder.Unwrap(node.Value) as WorkflowInputBuilder;
                    return inputBuilder != null && inputBuilder.Index == 0;
                });

                if (inputNode != null)
                {
                    var concatBuilder = new CombinatorBuilder
                    {
                        Combinator = new Bonsai.Reactive.Concat()
                    };

                    var concatNode = workflow.Add(concatBuilder);
                    foreach (var successor in inputNode.Successors)
                    {
                        concatNode.Successors.Add(successor);
                    }

                    inputNode.Successors.Clear();
                    workflow.AddEdge(inputNode, concatNode, new ExpressionBuilderArgument());
                }
            }
        }
    }
}
