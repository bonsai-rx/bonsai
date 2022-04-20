using Bonsai.Expressions;
using Bonsai.Dag;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;

namespace Bonsai.Editor
{
    static class UpgradeHelper
    {
        static readonly SemanticVersion DeprecationTarget = SemanticVersion.Parse("2.4.0");
        static readonly SemanticVersion RemoveMemberSelectorPrefixVersion = SemanticVersion.Parse("2.4.0-preview");
        static readonly SemanticVersion EnumerableUnfoldingVersion = SemanticVersion.Parse("2.3.0");
        const string MemberSelectorPrefix = ExpressionBuilderArgument.ArgumentNamePrefix + ".";
        const string IndexerSelectorPrefix = ExpressionBuilderArgument.ArgumentNamePrefix + "[";

        static string RemoveMemberSelectorPrefix(string selector)
        {
            if (string.IsNullOrEmpty(selector)) return selector;
            var memberNames = ExpressionHelper
                            .SelectMemberNames(selector)
                            .Select(name => name.Replace(".Item[", "["))
                            .Select(name => name == ExpressionBuilderArgument.ArgumentNamePrefix
                                ? ExpressionHelper.ImplicitParameterName
                                : name.IndexOf(MemberSelectorPrefix) == 0
                                ? name.Substring(MemberSelectorPrefix.Length)
                                : name.IndexOf(IndexerSelectorPrefix) == 0
                                ? ExpressionHelper.ImplicitParameterName + name.Substring(IndexerSelectorPrefix.Length - 1)
                                : name)
                            .ToArray();
            return string.Join(ExpressionHelper.ArgumentSeparator, memberNames);
        }

        internal static bool IsDeprecated(SemanticVersion version)
        {
            return version < DeprecationTarget;
        }

        static void GetArgumentCount(ExpressionBuilderGraph workflow, Dictionary<ExpressionBuilder, int> argumentCount)
        {
            foreach (var node in workflow.TopologicalSort())
            {
                foreach (var successor in node.Successors)
                {
                    argumentCount.TryGetValue(successor.Target.Value, out int count);
                    argumentCount[successor.Target.Value] = count + 1;
                }

                if (node.Value is WorkflowExpressionBuilder workflowExpression)
                {
                    GetArgumentCount(workflowExpression.Workflow, argumentCount);
                }
            }
        }

        internal static ExpressionBuilderGraph UpgradeBuilderNodes(ExpressionBuilderGraph workflow, SemanticVersion version)
        {
            var argumentCount = new Dictionary<ExpressionBuilder, int>();
            GetArgumentCount(workflow, argumentCount);
            return workflow.Convert(builder =>
            {
#pragma warning disable CS0612 // Type or member is obsolete
                if (builder is SourceBuilder sourceBuilder)
                {
                    return new CombinatorBuilder
                    {
                        Combinator = sourceBuilder.Generator
                    };
                }
#pragma warning restore CS0612 // Type or member is obsolete

#pragma warning disable CS0612 // Type or member is obsolete
                if (builder is WindowWorkflowBuilder windowWorkflowBuilder)
                {
                    return new CreateObservableBuilder(windowWorkflowBuilder.Workflow)
                    {
                        Name = windowWorkflowBuilder.Name,
                        Description = windowWorkflowBuilder.Description
                    };
                }
#pragma warning restore CS0612 // Type or member is obsolete

                if (builder is ExternalizedProperty property)
                {
                    if (argumentCount.TryGetValue(property, out int count))
                    {
                        var mapping = new PropertyMapping();
                        mapping.Name = property.MemberName;
                        return new PropertyMappingBuilder { PropertyMappings = { mapping } };
                    }
                    else
                    {
                        var mapping = new ExternalizedMapping();
                        mapping.Name = property.MemberName;
                        if (property.Name != property.MemberName)
                        {
                            mapping.DisplayName = property.Name;
                        }
                        return new ExternalizedMappingBuilder { ExternalizedProperties = { mapping } };
                    }
                }

                var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
                if (workflowElement is ParseBuilder parse && version < RemoveMemberSelectorPrefixVersion)
                {
                    return new ParseBuilder
                    {
                        Pattern = parse.Pattern.Replace("%p", "%T")
                    };
                }

#pragma warning disable CS0612 // Type or member is obsolete
                if (workflowElement is Reactive.Index index)
                {
                    return new CombinatorBuilder
                    {
                        Combinator = new Reactive.ElementIndex()
                    };
                }
#pragma warning restore CS0612 // Type or member is obsolete

                if (workflowElement is ExpressionBuilder builderElement && version < RemoveMemberSelectorPrefixVersion)
                {
                    if (builderElement is PropertyMappingBuilder mappingBuilder)
                    {
                        foreach (var mapping in mappingBuilder.PropertyMappings)
                        {
                            mapping.Selector = RemoveMemberSelectorPrefix(mapping.Selector);
                        }

                        if (mappingBuilder is InputMappingBuilder inputMapping)
                        {
                            inputMapping.Selector = RemoveMemberSelectorPrefix(inputMapping.Selector);
                        }
                    }
                    else
                    {
                        var properties = from selectorProperty in TypeDescriptor.GetProperties(builderElement).Cast<PropertyDescriptor>()
                                         where selectorProperty.PropertyType == typeof(string)
                                         let editorAttribute = (EditorAttribute)selectorProperty.Attributes[typeof(EditorAttribute)]
                                         where editorAttribute != null &&
                                               editorAttribute.EditorTypeName.StartsWith("Bonsai.Design.MultiMemberSelectorEditor, Bonsai.Design")
                                         select selectorProperty;
                        foreach (var selectorProperty in properties)
                        {
                            var selector = (string)selectorProperty.GetValue(builderElement);
                            selectorProperty.SetValue(builderElement, RemoveMemberSelectorPrefix(selector));
                        }
                    }

                    var builderType = builderElement.GetType();
                    if (builderType.FullName == "Bonsai.IO.CsvWriter")
                    {
                        var compatibilityMode = builderType.GetProperty("CompatibilityMode");
                        if (compatibilityMode != null)
                        {
                            compatibilityMode.SetValue(builderElement, true);
                        }
                    }
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
                if (node.Value is WorkflowExpressionBuilder workflowBuilder)
                {
                    foreach (var dependency in GetEnumerableUpgradeTargets(workflowBuilder.Workflow))
                    {
                        yield return dependency;
                    }
                }

                var successor = node.Successors.FirstOrDefault(edge => edge.Label.Index == 0 &&
                                                                       (edge.Target.Value is SelectManyBuilder ||
#pragma warning disable CS0612 // Type or member is obsolete
                                                                        edge.Target.Value is WindowWorkflowBuilder));
#pragma warning restore CS0612 // Type or member is obsolete
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
                    return ExpressionBuilder.Unwrap(node.Value) is WorkflowInputBuilder inputBuilder && inputBuilder.Index == 0;
                });

                if (inputNode != null)
                {
                    var mergeBuilder = new CombinatorBuilder
                    {
                        Combinator = new Reactive.Merge()
                    };

                    var mergeNode = workflow.Add(mergeBuilder);
                    foreach (var successor in inputNode.Successors)
                    {
                        mergeNode.Successors.Add(successor);
                    }

                    inputNode.Successors.Clear();
                    workflow.AddEdge(inputNode, mergeNode, new ExpressionBuilderArgument());
                }
            }
        }
    }
}
