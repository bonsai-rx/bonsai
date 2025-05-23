﻿using Bonsai.Expressions;
using Bonsai.Dag;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System;
using System.IO;
using System.Xml;

namespace Bonsai.Editor.GraphModel
{
    static class UpgradeHelper
    {
        static readonly SemanticVersion DeprecationTarget = SemanticVersion.Parse("2.9.0");
        static readonly SemanticVersion RetargetScriptingPackageVersion = SemanticVersion.Parse("2.7.0");
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

        static bool IsDeprecated(SemanticVersion version)
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

        static Stream GetWorkflowStream(string path)
        {
            //TODO: Consider refactoring into core API
            const char AssemblySeparator = ':';
            var separatorIndex = path.IndexOf(AssemblySeparator);
            var embeddedResource = separatorIndex >= 0 && !Path.IsPathRooted(path);
            if (embeddedResource)
            {
                var nameElements = path.Split(new[] { AssemblySeparator }, 2);
                if (string.IsNullOrEmpty(nameElements[0])) return null;

                var assembly = System.Reflection.Assembly.Load(nameElements[0]);
                var resourceName = string.Join(ExpressionHelper.MemberSeparator, nameElements);
                var workflowStream = assembly.GetManifestResourceStream(resourceName);
                return workflowStream;
            }
            else return File.Exists(path) ? File.OpenRead(path) : null;
        }

        static ExpressionBuilder ConvertBuilder(ExpressionBuilder builder, Func<ExpressionBuilder, ExpressionBuilder> selector)
        {
            //TODO: Remove this workaround for ensuring workflow node order (refactor core conversion API)
            var set = new[] { new Node<ExpressionBuilder, ExpressionBuilderArgument>(builder) };
            var result = set.Convert(selector, recurse: false);
            return result.First().Value;
        }

        internal static bool TryUpgradeWorkflow(ExpressionBuilderGraph workflow, string fileName, out ExpressionBuilderGraph upgradedWorkflow)
        {
            Stream workflowStream;
            try { workflowStream = GetWorkflowStream(fileName); }
            catch (SystemException) { workflowStream = null; }

            if (workflowStream != null)
            {
                using var reader = XmlReader.Create(workflowStream, new XmlReaderSettings { CloseInput = true });
                ElementStore.ReadWorkflowVersion(reader, out SemanticVersion version);
                return TryUpgradeWorkflow(workflow, version, out upgradedWorkflow);
            }

            upgradedWorkflow = workflow;
            return false;
        }

        internal static bool TryUpgradeWorkflow(ExpressionBuilderGraph workflow, SemanticVersion version, out ExpressionBuilderGraph upgradedWorkflow)
        {
            var upgraded = false;
            upgradedWorkflow = workflow;
            if (workflow.Count > 0 && (version == null || IsDeprecated(version)))
            {
                upgraded |= TryUpgradeEnumerableUnfoldingRules(workflow, version);
                upgraded |= TryUpgradeBuilderNodes(workflow, version, out upgradedWorkflow);
            }

            return upgraded;
        }

        static bool TryUpgradeBuilderNodes(ExpressionBuilderGraph workflow, SemanticVersion version, out ExpressionBuilderGraph upgradedWorkflow)
        {
            var upgraded = false;
            var argumentCount = new Dictionary<ExpressionBuilder, int>();
            GetArgumentCount(workflow, argumentCount);
            ExpressionBuilder UpgradeBuilder(ExpressionBuilder builder)
            {
                if (builder is DisableBuilder disableBuilder)
                {
                    var upgradedBuilder = UpgradeBuilder(disableBuilder.Builder);
                    if (upgradedBuilder != disableBuilder.Builder)
                    {
                        upgradedBuilder = ConvertBuilder(disableBuilder, builder => upgradedBuilder);
                        return new DisableBuilder(upgradedBuilder);
                    }

                    return builder;
                }

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

                var elementType = workflowElement.GetType();
                if (elementType.Namespace == "Bonsai.Scripting" && version < RetargetScriptingPackageVersion)
                {
                    if (elementType.Name.Contains("Expression"))
                    {
                        var replacementTypeName = elementType.AssemblyQualifiedName.Replace("Bonsai.Scripting", "Bonsai.Scripting.Expressions");
                        var replacementType = Type.GetType(replacementTypeName);
                        if (replacementType != null)
                        {
                            dynamic legacyElement = workflowElement;
                            dynamic element = Activator.CreateInstance(replacementType);
                            element.Name = legacyElement.Name;
                            element.Description = legacyElement.Description;
                            element.Expression = legacyElement.Expression;
                            return element;
                        }
                    }
                    else if (elementType.Name.Contains("Python"))
                    {
                        var replacementTypeName = elementType.AssemblyQualifiedName.Replace("Bonsai.Scripting", "Bonsai.Scripting.IronPython");
                        var replacementType = Type.GetType(replacementTypeName);
                        if (replacementType != null)
                        {
                            dynamic legacyElement = workflowElement;
                            dynamic element = Activator.CreateInstance(replacementType);
                            element.Name = legacyElement.Name;
                            element.Description = legacyElement.Description;
                            element.Script = legacyElement.Script;
                            return element;
                        }
                    }
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
                    upgraded = true;
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

                if (workflowElement is WorkflowExpressionBuilder workflowBuilder &&
                    Attribute.IsDefined(elementType, typeof(ProxyTypeAttribute)))
                {
                    var proxyType = Attribute.GetCustomAttribute(elementType, typeof(ProxyTypeAttribute)) as ProxyTypeAttribute;
                    var replacementBuilder = (WorkflowExpressionBuilder)Activator.CreateInstance(proxyType.Destination, workflowBuilder.Workflow);
                    replacementBuilder.Name = workflowBuilder.Name;
                    replacementBuilder.Description = workflowBuilder.Description;
                    return replacementBuilder;
                }

                if (workflowElement is SubjectExpressionBuilder subjectBuilder &&
                    Attribute.IsDefined(elementType, typeof(ProxyTypeAttribute)))
                {
                    SubjectExpressionBuilder replacementBuilder;
                    var proxyType = ((ProxyTypeAttribute)Attribute.GetCustomAttribute(elementType, typeof(ProxyTypeAttribute)))?.Destination;
                    if (elementType.IsGenericType)
                    {
                        var subjectType = elementType.GenericTypeArguments;
                        proxyType = proxyType.MakeGenericType(subjectType);
                    }
                    replacementBuilder = (SubjectExpressionBuilder)Activator.CreateInstance(proxyType);
                    replacementBuilder.Name = subjectBuilder.Name;
#pragma warning disable CS0612 // Type or member is obsolete
                    if (elementType.Name.StartsWith(nameof(ReplaySubjectBuilder)))
                    {
                        dynamic legacyElement = workflowElement;
                        dynamic element = replacementBuilder;
                        element.BufferSize = legacyElement.BufferSize;
                        element.Window = legacyElement.Window;
                    }
#pragma warning restore CS0612 // Type or member is obsolete
                    return replacementBuilder;
                }

                return builder;
            };

            upgradedWorkflow = workflow.Convert(builder =>
            {
                var upgradedBuilder = UpgradeBuilder(builder);
                if (upgradedBuilder != builder) upgraded = true;
                return upgradedBuilder;
            });
            return upgraded;
        }

        static bool TryUpgradeEnumerableUnfoldingRules(ExpressionBuilderGraph workflow, SemanticVersion version)
        {
            var upgraded = false;
            if (version < EnumerableUnfoldingVersion)
            {
                var upgradeTargets = GetEnumerableUpgradeTargets(workflow).ToList();
                foreach (var upgradeTarget in upgradeTargets)
                {
                    UpgradeEnumerableInputDependency(workflow, upgradeTarget);
                    upgraded = true;
                }
            }

            return upgraded;
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
#pragma warning disable CS0612 // Type or member is obsolete
                                                                       (edge.Target.Value is SelectManyBuilder ||
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

        static void UpgradeEnumerableInputDependency(ExpressionBuilderGraph workflow, WorkflowInputDependency inputDependency)
        {
            var dependency = workflow.Build(inputDependency.Dependency);
            var sourceType = dependency.Type.GetGenericArguments()[0];
            if (ExpressionHelper.IsEnumerableType(sourceType) && sourceType != typeof(string))
            {
                var targetWorkflow = inputDependency.Target;
                var inputNode = targetWorkflow.FirstOrDefault(node =>
                {
                    return ExpressionBuilder.Unwrap(node.Value) is WorkflowInputBuilder inputBuilder && inputBuilder.Index == 0;
                });

                if (inputNode != null)
                {
                    var mergeBuilder = new CombinatorBuilder
                    {
                        Combinator = new Reactive.Merge()
                    };

                    var mergeNode = targetWorkflow.Add(mergeBuilder);
                    foreach (var successor in inputNode.Successors)
                    {
                        mergeNode.Successors.Add(successor);
                    }

                    inputNode.Successors.Clear();
                    targetWorkflow.AddEdge(inputNode, mergeNode, new ExpressionBuilderArgument());
                }
            }
        }
    }
}
