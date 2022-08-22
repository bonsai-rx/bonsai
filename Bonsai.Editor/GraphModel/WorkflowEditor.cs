using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Editor.Properties;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Subjects;

namespace Bonsai.Editor.GraphModel
{
    class WorkflowEditor
    {
        static readonly Action EmptyAction = () => { };
        readonly CommandExecutor commandExecutor;
        readonly IServiceProvider serviceProvider;
        readonly IGraphView graphView;
        readonly Subject<Exception> error;
        readonly Subject<bool> updateLayout;
        readonly Subject<bool> updateParentLayout;
        readonly Subject<bool> invalidateLayout;
        readonly Subject<IEnumerable<ExpressionBuilder>> updateSelection;
        readonly Subject<IWorkflowExpressionBuilder> closeWorkflowEditor;

        public WorkflowEditor(IServiceProvider provider, IGraphView view)
        {
            graphView = view ?? throw new ArgumentNullException(nameof(view));
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            error = new Subject<Exception>();
            updateLayout = new Subject<bool>();
            updateParentLayout = new Subject<bool>();
            invalidateLayout = new Subject<bool>();
            updateSelection = new Subject<IEnumerable<ExpressionBuilder>>();
            closeWorkflowEditor = new Subject<IWorkflowExpressionBuilder>();
        }

        public ExpressionBuilderGraph Workflow { get; set; }

        public IObservable<Exception> Error => error;

        public IObservable<bool> UpdateLayout => updateLayout;

        public IObservable<bool> UpdateParentLayout => updateParentLayout;

        public IObservable<bool> InvalidateLayout => invalidateLayout;

        public IObservable<IEnumerable<ExpressionBuilder>> UpdateSelection => updateSelection;

        public IObservable<IWorkflowExpressionBuilder> CloseWorkflowEditor => closeWorkflowEditor;

        private static Node<ExpressionBuilder, ExpressionBuilderArgument> FindWorkflowValue(ExpressionBuilderGraph workflow, ExpressionBuilder value)
        {
            return workflow.Single(n => ExpressionBuilder.Unwrap(n.Value) == value);
        }

        private void AddWorkflowNode(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            workflow.Add(node);
            if (ExpressionBuilder.Unwrap(node.Value) is WorkflowInputBuilder workflowInput)
            {
                foreach (var inputBuilder in workflow.Select(xs => ExpressionBuilder.Unwrap(xs.Value) as WorkflowInputBuilder)
                                                     .Where(xs => xs != null))
                {
                    if (inputBuilder != workflowInput && inputBuilder.Index >= workflowInput.Index)
                    {
                        inputBuilder.Index++;
                    }
                }

                updateParentLayout.OnNext(false);
            }
        }

        private void RemoveWorkflowNode(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
            workflow.Remove(node);
            if (ExpressionBuilder.Unwrap(node.Value) is WorkflowInputBuilder workflowInput)
            {
                foreach (var inputBuilder in workflow.Select(xs => ExpressionBuilder.Unwrap(xs.Value) as WorkflowInputBuilder)
                                                     .Where(xs => xs != null))
                {
                    if (inputBuilder.Index > workflowInput.Index)
                    {
                        inputBuilder.Index--;
                    }
                }

                updateParentLayout.OnNext(false);
            }
        }

        private Action CreateUpdateGraphLayoutDelegate()
        {
            return () => updateLayout.OnNext(true);
        }

        private Action CreateInvalidateGraphLayoutDelegate()
        {
            return () => invalidateLayout.OnNext(true);
        }

        private Action CreateUpdateSelectionDelegate()
        {
            return CreateUpdateSelectionDelegate(Enumerable.Empty<ExpressionBuilder>());
        }

        private Action CreateUpdateSelectionDelegate(GraphNode selection)
        {
            var selectedNodes = selection == null ? Enumerable.Empty<GraphNode>() : new[] { selection };
            return CreateUpdateSelectionDelegate(selectedNodes);
        }

        private Action CreateUpdateSelectionDelegate(IEnumerable<GraphNode> selection)
        {
            var nodes = selection.Select(node => GetGraphNodeTag(Workflow, node));
            return CreateUpdateSelectionDelegate(nodes);
        }

        private Action CreateUpdateSelectionDelegate(Node<ExpressionBuilder, ExpressionBuilderArgument> selection)
        {
            var selectedNodes = selection == null ? Enumerable.Empty<Node<ExpressionBuilder, ExpressionBuilderArgument>>() : new[] { selection };
            return CreateUpdateSelectionDelegate(selectedNodes);
        }

        private Action CreateUpdateSelectionDelegate(IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> selection)
        {
            return CreateUpdateSelectionDelegate(selection.Select(node => node.Value));
        }

        private Action CreateUpdateSelectionDelegate(ExpressionBuilder selection)
        {
            return CreateUpdateSelectionDelegate(new[] { selection });
        }

        private Action CreateUpdateSelectionDelegate(IEnumerable<ExpressionBuilder> selection)
        {
            return () => updateSelection.OnNext(selection);
        }

        public static ExpressionBuilder GetGraphNodeBuilder(GraphNode node)
        {
            if (node != null && node.Value != null)
            {
                return ExpressionBuilder.Unwrap(node.Value);
            }

            return null;
        }

        public static Node<ExpressionBuilder, ExpressionBuilderArgument> GetGraphNodeTag(ExpressionBuilderGraph workflow, GraphNode node)
        {
            return GetGraphNodeTag(workflow, node, true);
        }

        public static Node<ExpressionBuilder, ExpressionBuilderArgument> GetGraphNodeTag(ExpressionBuilderGraph workflow, GraphNode node, bool throwOnError)
        {
            while (node.Value == null)
            {
                var edge = (GraphEdge)node.Tag;
                node = edge.Node;
            }

            var nodeTag = (Node<ExpressionBuilder, ExpressionBuilderArgument>)node.Tag;
            if (throwOnError) return workflow.First(ns => ns.Value == nodeTag.Value);
            else return workflow.FirstOrDefault(ns => ns.Value == nodeTag.Value);
        }

        static IEnumerable<ExpressionBuilder> GetBuilderArguments(ExpressionBuilderGraph workflow, ExpressionBuilder builder)
        {
            if (workflow == null)
            {
                throw new ArgumentNullException(nameof(workflow));
            }

            var activation = new HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>>();
            foreach (var node in workflow.TopologicalSort())
            {
                if (node.Value.IsBuildDependency()) continue;
                if (!activation.Contains(node))
                {
                    if (ExpressionBuilder.Unwrap(node.Value) is DisableBuilder)
                    {
                        continue;
                    }
                    else activation.Add(node);
                }

                foreach (var successor in node.Successors)
                {
                    if (successor.Target.Value == builder)
                    {
                        yield return node.Value;
                    }

                    activation.Add(successor.Target);
                }
            }
        }

        GraphCommand GetInsertGraphNodeCommands(
            Node<ExpressionBuilder, ExpressionBuilderArgument> sourceNode,
            Node<ExpressionBuilder, ExpressionBuilderArgument> sinkNode,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> targetNodes,
            CreateGraphNodeType nodeType,
            bool branch,
            bool validate = true)
        {
            var workflow = this.Workflow;
            var addConnection = EmptyAction;
            var removeConnection = EmptyAction;
            if (nodeType == CreateGraphNodeType.Predecessor)
            {
                var index = 0;
                foreach (var node in targetNodes)
                {
                    // Ensure we can connect to the selected node
                    var mappingBuilder = ExpressionBuilder.Unwrap(node.Value) as ExternalizedMappingBuilder;
                    var maxConnectionCount = mappingBuilder != null ? 1 : node.Value.ArgumentRange.UpperBound;
                    if (!validate || maxConnectionCount > 0)
                    {
                        var parameter = new ExpressionBuilderArgument();
                        var predecessors = workflow.PredecessorEdges(node).ToList();
                        if (index > 0 || branch || sourceNode.Value.ArgumentRange.UpperBound == 0)
                        {
                            // Resolve predecessors only for the first selected target node, if we are not branching
                            parameter.Index = predecessors.Count;
                        }
                        else
                        {
                            // If we have predecessors, we need to connect the new node in the right branches
                            foreach (var predecessor in predecessors)
                            {
                                var predecessorEdge = predecessor.Item2;
                                var predecessorNode = predecessor.Item1;
                                var edgeIndex = predecessor.Item3;
                                addConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, sourceNode, predecessorEdge.Label); };
                                removeConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, predecessorEdge); };
                            }
                        }

                        // After dealing with predecessors, we just create an edge to the selected node
                        var edge = Edge.Create(node, parameter);
                        addConnection += () => { workflow.AddEdge(sinkNode, edge); };
                        removeConnection += () => { workflow.RemoveEdge(sinkNode, edge); };
                    }

                    index++;
                }
            }
            else if (!validate || sourceNode.Value.ArgumentRange.UpperBound > 0)
            {
                var index = 0;
                foreach (var node in targetNodes)
                {
                    var parameter = new ExpressionBuilderArgument(index++);
                    if (parameter.Index == 0 && !branch && node.Successors.Count > 0)
                    {
                        // If we are not creating a new branch, the new node will inherit all branches of the first selected node
                        var edge = Edge.Create(sourceNode, parameter);
                        var oldSuccessors = node.Successors.ToArray();
                        addConnection += () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(node, successor);
                                workflow.AddEdge(sinkNode, successor);
                            }
                            workflow.AddEdge(node, edge);
                        };

                        removeConnection += () =>
                        {
                            foreach (var successor in oldSuccessors)
                            {
                                workflow.RemoveEdge(sinkNode, successor);
                                workflow.AddEdge(node, successor);
                            }
                            workflow.RemoveEdge(node, edge);
                        };
                    }
                    else
                    {
                        // Otherwise, just create a new branch
                        var edge = Edge.Create(sourceNode, parameter);
                        addConnection += () => { workflow.AddEdge(node, edge); };
                        removeConnection += () => { workflow.RemoveEdge(node, edge); };
                    }
                }
            }

            return new GraphCommand(addConnection, removeConnection);
        }

        bool CanConnect(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var target = GetGraphNodeTag(Workflow, graphViewTarget, false);
            var targetElement = target != null ? ExpressionBuilder.Unwrap(target.Value) : null;
            var isExternalizedMapping = targetElement is ExternalizedMappingBuilder;
            var maxConnectionCount = isExternalizedMapping ? 1 : target.Value.ArgumentRange.UpperBound;
            var sources = graphViewSources.Select(sourceNode => GetGraphNodeTag(Workflow, sourceNode, false));
            var connectionCount = Workflow.Contains(target) ? GetBuilderArguments(Workflow, target.Value).Count() : 0;
            foreach (var source in sources)
            {
                if (source == null || target == source || source.Successors.Any(edge => edge.Target == target) ||
                    isExternalizedMapping && source.Value.IsBuildDependency())
                {
                    return false;
                }

                if (connectionCount++ >= maxConnectionCount &&
                    !source.Value.IsBuildDependency() ||
                    target.DepthFirstSearch().Contains(source))
                {
                    return false;
                }
            }

            return true;
        }

        bool CanDisconnect(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var target = GetGraphNodeTag(Workflow, graphViewTarget, false);
            foreach (var sourceNode in graphViewSources)
            {
                var node = GetGraphNodeTag(Workflow, sourceNode, false);
                if (node == null) return false;

                if (!node.Successors.Any(edge => edge.Target == target))
                {
                    return false;
                }
            }

            return true;
        }

        bool CanReorder(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var target = GetGraphNodeTag(Workflow, graphViewTarget, false);
            if (target == null) return false;

            var targetSuccessors = target.Successors.Select(edge => edge.Target);
            foreach (var sourceNode in graphViewSources)
            {
                var node = GetGraphNodeTag(Workflow, sourceNode, false);
                if (node == null) return false;

                var nodeSuccessors = node.Successors.Select(edge => edge.Target);
                if (!nodeSuccessors.Intersect(targetSuccessors).Any() &&
                    node.DepthFirstSearch().Intersect(target.DepthFirstSearch()).Any())
                {
                    return false;
                }
            }

            return true;
        }

        public bool ValidateConnection(bool branch, bool shift, IEnumerable<GraphNode> nodes, GraphNode target)
        {
            if (branch) return CanReorder(nodes, target);
            else if (shift) return CanDisconnect(nodes, target);
            else return CanConnect(nodes, target);
        }

        PropertyMappingBuilder ConvertExternalizedMapping(ExternalizedMappingBuilder mappingBuilder)
        {
            var builder = new PropertyMappingBuilder();
            foreach (var mapping in mappingBuilder.ExternalizedProperties)
            {
                builder.PropertyMappings.Add(new PropertyMapping(mapping.Name, null));
            }

            return builder;
        }

        void ReplaceExternalizedMappings(CreateGraphNodeType nodeType, GraphNode[] targetNodes)
        {
            if (nodeType == CreateGraphNodeType.Predecessor)
            {
                for (int i = 0; i < targetNodes.Length; i++)
                {
                    if (GetGraphNodeBuilder(targetNodes[i]) is ExternalizedMappingBuilder mappingBuilder)
                    {
                        var propertyMappingBuilder = ConvertExternalizedMapping(mappingBuilder);
                        ReplaceNode(targetNodes[i], propertyMappingBuilder);
                    }
                }
            }
        }

        public void ConnectGraphNodes(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            if (GetGraphNodeBuilder(graphViewTarget) is ExternalizedMappingBuilder mappingBuilder)
            {
                var propertyMappingBuilder = ConvertExternalizedMapping(mappingBuilder);
                var restoreSelectedNodes = CreateUpdateSelectionDelegate(graphViewSources);
                var selectCreatedNode = CreateUpdateSelectionDelegate(propertyMappingBuilder);
                var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
                commandExecutor.BeginCompositeCommand();
                commandExecutor.Execute(EmptyAction, updateGraphLayout + restoreSelectedNodes);
                ConnectGraphNodes(graphViewSources, graphViewTarget, validate: false);
                ReplaceNode(graphViewTarget, propertyMappingBuilder);
                commandExecutor.Execute(updateGraphLayout + selectCreatedNode, EmptyAction);
                commandExecutor.EndCompositeCommand();
            }
            else ConnectGraphNodes(graphViewSources, graphViewTarget, validate: true);
        }

        private void ConnectInternalNodes(GraphNode source, GraphNode target)
        {
            ConnectGraphNodes(new[] { source }, target, validate: false);
        }

        static void FindNextIndex(ref int index, ref int offset, int[] indices)
        {
            for (; offset < indices.Length; offset++, index++)
            {
                if (indices[offset] > index) break;
            }
        }

        private void ConnectGraphNodes(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget, bool validate)
        {
            var workflow = this.Workflow;
            var addConnection = EmptyAction;
            var removeConnection = EmptyAction;
            var target = GetGraphNodeTag(workflow, graphViewTarget);
            var sortedPredecessors = workflow.PredecessorEdges(target)
                .Select(edge => edge.Item2.Label.Index)
                .OrderBy(idx => idx).ToArray();

            var offset = 0;
            var connectionIndex = 0;
            foreach (var graphViewSource in graphViewSources)
            {
                FindNextIndex(ref connectionIndex, ref offset, sortedPredecessors);
                var source = GetGraphNodeTag(workflow, graphViewSource);
                var parameter = new ExpressionBuilderArgument(connectionIndex);
                var edge = Edge.Create(target, parameter);
                addConnection += () => workflow.AddEdge(source, edge);
                removeConnection += () => workflow.RemoveEdge(source, edge);
                connectionIndex++;
            }

            Action restoreSelectedNodes, updateSelectedNode, updateGraphLayout;
            if (!validate) restoreSelectedNodes = updateSelectedNode = updateGraphLayout = null;
            else
            {
                restoreSelectedNodes = CreateUpdateSelectionDelegate(graphViewSources);
                updateSelectedNode = CreateUpdateSelectionDelegate(graphViewTarget);
                updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            }

            commandExecutor.Execute(
            () =>
            {
                addConnection();
                if (validate)
                {
                    updateGraphLayout();
                    updateSelectedNode();
                }
            },
            () =>
            {
                removeConnection();
                if (validate)
                {
                    updateGraphLayout();
                    restoreSelectedNodes();
                }
            });
        }

        public void DisconnectGraphNodes(IEnumerable<GraphNode> graphViewSources, GraphNode graphViewTarget)
        {
            var workflow = this.Workflow;
            var addConnection = EmptyAction;
            var removeConnection = EmptyAction;
            var target = GetGraphNodeTag(workflow, graphViewTarget);
            var predecessorEdges = workflow.PredecessorEdges(target).ToArray();
            foreach (var graphViewSource in graphViewSources)
            {
                var source = GetGraphNodeTag(workflow, graphViewSource);
                var predecessor = predecessorEdges.Where(xs => xs.Item1 == source).FirstOrDefault();
                if (predecessor == null) continue;
                var edge = predecessor.Item2;
                var edgeIndex = edge.Label.Index;
                var siblingEdgesAfter = (from siblingEdge in predecessorEdges
                                         where siblingEdge.Item2.Label.Index.CompareTo(edgeIndex) > 0
                                         select siblingEdge.Item2)
                                         .ToArray();

                addConnection += () =>
                {
                    predecessor.Item1.Successors.Insert(predecessor.Item3, edge);
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index++;
                    }
                };

                removeConnection += () =>
                {
                    predecessor.Item1.Successors.RemoveAt(predecessor.Item3);
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index--;
                    }
                };
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            commandExecutor.Execute(
            () =>
            {
                removeConnection();
                updateGraphLayout();
            },
            () =>
            {
                addConnection();
                updateGraphLayout();
            });
        }

        void ReorderGraphNode(
            Node<ExpressionBuilder, ExpressionBuilderArgument> source,
            ref Node<ExpressionBuilder, ExpressionBuilderArgument> target,
            List<Node<ExpressionBuilder, ExpressionBuilderArgument>> reorderCommands,
            HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>> selectedElements)
        {
            var workflow = this.Workflow;
            var reorderConnection = EmptyAction;
            var restoreConnection = EmptyAction;
            var commonSuccessors = from sourceEdge in source.Successors
                                   join targetEdge in target.Successors on sourceEdge.Target equals targetEdge.Target
                                   select new { sourceEdge, targetEdge };
            foreach (var commonSuccessor in commonSuccessors)
            {
                var successor = commonSuccessor.sourceEdge.Target;
                var sourceLabel = commonSuccessor.sourceEdge.Label;
                var targetLabel = commonSuccessor.targetEdge.Label;
                var siblingEdges = workflow.PredecessorEdges(successor).Select(predecessor => predecessor.Item2);
                var sourceLabelIndex = sourceLabel.Index;
                if (sourceLabelIndex < targetLabel.Index) // decrement sibling labels
                {
                    var shiftedEdges = siblingEdges.Where(edge => edge.Label.Index > sourceLabelIndex && edge.Label.Index < targetLabel.Index).ToArray();
                    reorderConnection += () =>
                    {
                        sourceLabel.Index = targetLabel.Index - 1;
                        Array.ForEach(shiftedEdges, edge => edge.Label.Index--);
                    };

                    restoreConnection += () =>
                    {
                        sourceLabel.Index = sourceLabelIndex;
                        Array.ForEach(shiftedEdges, edge => edge.Label.Index++);
                    };
                }
                else // increment sibling labels
                {
                    var shiftedEdges = siblingEdges.Where(edge => edge.Label.Index < sourceLabelIndex && edge.Label.Index >= targetLabel.Index).ToArray();
                    reorderConnection += () =>
                    {
                        sourceLabel.Index = targetLabel.Index;
                        Array.ForEach(shiftedEdges, edge => edge.Label.Index++);
                    };

                    restoreConnection += () =>
                    {
                        sourceLabel.Index = sourceLabelIndex;
                        Array.ForEach(shiftedEdges, edge => edge.Label.Index--);
                    };
                }
            }

            if (reorderConnection != EmptyAction)
            {
                // reorder node connections
                commandExecutor.Execute(reorderConnection, restoreConnection);
            }
            else
            {
                var targetNode = target;
                var components = workflow.FindConnectedComponents();
                var sourceComponent = components.First(component => component.Contains(source));
                var targetComponent = components.First(component => component.Contains(targetNode));
                if (sourceComponent == targetComponent) // reorder branches
                {
                    // find common ancestor
                    var sourceTrace = new { node = default(GraphNode), index = 0 };
                    var targetTrace = new { node = default(GraphNode), index = 0 };
                    var layering = sourceComponent.LongestPathLayering();
                    foreach (var node in layering.SelectMany(layer => layer))
                    {
                        var i = -1;
                        if (sourceTrace.node == null && node.Value == source.Value) sourceTrace = new { node, index = i };
                        if (targetTrace.node == null && node.Value == target.Value) targetTrace = new { node, index = i };
                        foreach (var successor in node.Successors)
                        {
                            i += 1;
                            if (successor.Node == sourceTrace.node) sourceTrace = new { node, index = i };
                            if (successor.Node == targetTrace.node) targetTrace = new { node, index = i };
                            if (sourceTrace.node != null && sourceTrace.node == targetTrace.node)
                            {
                                // common ancestor
                                var ancestor = GetGraphNodeTag(workflow, sourceTrace.node);
                                var sourceEdge = ancestor.Successors[sourceTrace.index];
                                commandExecutor.Execute(
                                () =>
                                {
                                    ancestor.Successors[sourceTrace.index] = null;
                                    ancestor.Successors.Insert(targetTrace.index, sourceEdge);
                                    ancestor.Successors.Remove(null);
                                },
                                () =>
                                {
                                    ancestor.Successors.RemoveAt(targetTrace.index);
                                    ancestor.Successors.Insert(sourceTrace.index, sourceEdge);
                                });
                                return;
                            }
                        }
                    }
                }
                else // reorder connected components
                {
                    var shiftedNodes = from index in Enumerable.Range(targetComponent.Index, components.Count - targetComponent.Index)
                                       let component = components[index]
                                       where component != sourceComponent
                                       from node in component
                                       select node;
                    var sourceBuilder = CloneWorkflowElements(sourceComponent);
                    var shiftedBuilder = CloneWorkflowElements(shiftedNodes);
                    foreach (var node in shiftedBuilder) sourceBuilder.Add(node);
                    foreach (var node in sourceComponent) DeleteGraphNode(node, true);
                    foreach (var node in shiftedNodes) DeleteGraphNode(node, true);
                    InsertGraphElements(sourceBuilder, new GraphNode[0], CreateGraphNodeType.Successor, false, EmptyAction, EmptyAction);

                    foreach (var pair in sourceComponent
                        .Concat(shiftedNodes)
                        .Zip(sourceBuilder, (element, clone) => new { element, clone }))
                    {
                        if (target == pair.element) target = pair.clone;
                        var index = reorderCommands.IndexOf(pair.element);
                        if (index >= 0)
                        {
                            reorderCommands[index] = pair.clone;
                        }

                        if (selectedElements.Contains(pair.element))
                        {
                            selectedElements.Remove(pair.element);
                            selectedElements.Add(pair.clone);
                        }
                    }
                }
            }
        }

        ExpressionBuilderGraph CloneWorkflowElements(IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> nodes)
        {
            var builder = new WorkflowBuilder(nodes.FromInspectableGraph(true));
            var markup = ElementStore.StoreWorkflowElements(builder);
            return ElementStore.RetrieveWorkflowElements(markup).Workflow.ToInspectableGraph();
        }

        IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> SortReorderCommands(IEnumerable<GraphNode> nodes, Node<ExpressionBuilder, ExpressionBuilderArgument> target)
        {
            var targetSuccessors = target.Successors.Select(edge => edge.Target);
            var components = Workflow.FindConnectedComponents();
            var targetComponent = components.First(component => component.Contains(target));

            List<Node<ExpressionBuilder, ExpressionBuilderArgument>> componentReordering = null;
            var sourceNodes = nodes.Select(node => GetGraphNodeTag(Workflow, node)).ToArray();
            for (int i = 0; i < sourceNodes.Length; i++)
            {
                var node = sourceNodes[i];
                if (node == null) continue;
                var nodeSuccessors = node.Successors.Select(edge => edge.Target);
                var allowChainReorder = nodeSuccessors.Intersect(targetSuccessors).Any();
                if (!allowChainReorder)
                {
                    var nodeComponent = components.First(component => component.Contains(node));
                    if (nodeComponent != targetComponent)
                    {
                        if (componentReordering == null) componentReordering = new List<Node<ExpressionBuilder, ExpressionBuilderArgument>>();
                        componentReordering.Add(node);
                        sourceNodes[i] = null;
                    }

                    for (int k = i + 1; k < sourceNodes.Length; k++)
                    {
                        var chainNode = sourceNodes[k];
                        if (chainNode == null) continue;
                        if (node.DepthFirstSearch().Intersect(chainNode.DepthFirstSearch()).Any() ||
                            nodeComponent != targetComponent &&
                            nodeComponent == components.First(component => component.Contains(chainNode)))
                        {
                            sourceNodes[k] = null;
                        }
                    }
                }
            }

            for (int i = 0; i < sourceNodes.Length; i++)
            {
                if (sourceNodes[i] != null)
                {
                    yield return sourceNodes[i];
                }
            }

            if (componentReordering != null)
            {
                foreach (var node in componentReordering)
                {
                    yield return node;
                }
            }
        }

        public void ReorderGraphNodes(IEnumerable<GraphNode> nodes, GraphNode target)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            var selectedNodes = new HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>>(nodes.Select(node => GetGraphNodeTag(Workflow, node)));
            if (selectedNodes.Count == 0) return;

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, updateGraphLayout + restoreSelectedNodes);

            var targetNode = GetGraphNodeTag(Workflow, target);
            var reorderCommands = SortReorderCommands(nodes, targetNode).ToList();
            for (int i = 0; i < reorderCommands.Count; i++)
            {
                var node = reorderCommands[i];
                try { ReorderGraphNode(node, ref targetNode, reorderCommands, selectedNodes); }
                catch (InvalidOperationException ex)
                {
                    var message = string.Format(Resources.ReorderGraphNodes_Error, ex.InnerException);
                    error.OnNext(new InvalidOperationException(message));
                    commandExecutor.EndCompositeCommand();
                    commandExecutor.Undo();
                    return;
                }
            }

            var updateSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);
            commandExecutor.Execute(updateGraphLayout + updateSelectedNodes, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        ExpressionBuilder CreateBuilder(string typeName, ElementCategory elementCategory, bool group)
        {
            if (elementCategory == ~ElementCategory.Workflow)
            {
                return new IncludeWorkflowBuilder { Path = typeName };
            }
            else if (elementCategory == ~ElementCategory.Source)
            {
                if (group) return new MulticastSubjectBuilder { Name = typeName };
                else return new SubscribeSubjectBuilder { Name = typeName };
            }

            var type = Type.GetType(typeName);
            if (type == null)
            {
                throw new ArgumentException(Resources.TypeNotFound_Error, nameof(typeName));
            }

            ExpressionBuilder builder;
            if (!type.IsSubclassOf(typeof(ExpressionBuilder)))
            {
                var element = Activator.CreateInstance(type);
                builder = ExpressionBuilder.FromWorkflowElement(element, elementCategory);
            }
            else builder = (ExpressionBuilder)Activator.CreateInstance(type);
            return builder;
        }

        WorkflowExpressionBuilder CreateWorkflowBuilder(string typeName, ExpressionBuilderGraph graph)
        {
            var type = Type.GetType(typeName);
            if (!typeof(WorkflowExpressionBuilder).IsAssignableFrom(type))
            {
                throw new ArgumentException(Resources.InvalidExpressionBuilderType_Error, nameof(typeName));
            }

            return (WorkflowExpressionBuilder)Activator.CreateInstance(type, graph);
        }

        static string MakeGenericType(string typeName, GraphNode selectedNode, out ElementCategory elementCategory)
        {
            var separatorToken = typeName.IndexOf(',');
            var genericTypeName = typeName.Substring(0, separatorToken) + "`1" + typeName.Substring(separatorToken);
            var genericType = Type.GetType(genericTypeName);
            if (genericType == null)
            {
                throw new ArgumentException(Resources.TypeNotFound_Error, nameof(typeName));
            }

            var inspectBuilder = (InspectBuilder)selectedNode.Value;
            var genericTypeAttributes = TypeDescriptor.GetAttributes(genericType);
            var elementCategoryAttribute = (WorkflowElementCategoryAttribute)genericTypeAttributes[typeof(WorkflowElementCategoryAttribute)];
            elementCategory = elementCategoryAttribute.Category;
            return genericType.MakeGenericType(inspectBuilder.ObservableType).AssemblyQualifiedName;
        }

        public void InsertGraphNode(string typeName, ElementCategory elementCategory, CreateGraphNodeType nodeType, bool branch, bool group)
        {
            InsertGraphNode(typeName, elementCategory, nodeType, branch, group, null);
        }

        public void InsertGraphNode(string typeName, ElementCategory elementCategory, CreateGraphNodeType nodeType, bool branch, bool group, string arguments)
        {
            if (string.IsNullOrEmpty(typeName))
            {
                throw new ArgumentNullException(nameof(typeName));
            }

            ExpressionBuilder builder;
            var selectedNodes = graphView.SelectedNodes.ToArray();
            var selectedNode = selectedNodes.Length > 0 ? selectedNodes[0] : null;
            if (group && selectedNode != null)
            {
                if (branch && selectedNodes.Length == 1)
                {
                    ReplaceGraphNode(selectedNode, typeName, elementCategory, arguments);
                    return;
                }
                else if (elementCategory == ~ElementCategory.Combinator)
                {
                    typeName = MakeGenericType(typeName, selectedNode, out elementCategory);
                }
                else if (elementCategory > ~ElementCategory.Source)
                {
                    GroupGraphNodes(selectedNodes, typeName);
                    selectedNode = graphView.SelectedNodes.First();
                    ConfigureBuilder(selectedNode.Value, selectedNode, arguments);
                    return;
                }
            }
            
            builder = CreateBuilder(typeName, elementCategory, group);
            ConfigureBuilder(builder, selectedNode, arguments);
            var externalizedMapping = typeName == typeof(ExternalizedMappingBuilder).AssemblyQualifiedName;
            if (externalizedMapping) nodeType = CreateGraphNodeType.Predecessor;
            var commands = GetCreateGraphNodeCommands(builder, selectedNodes, nodeType, branch);
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, commands.Item2.Undo);
            commandExecutor.Execute(commands.Item1.Command, commands.Item1.Undo);
            ReplaceExternalizedMappings(nodeType, selectedNodes);
            commandExecutor.Execute(commands.Item2.Command, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        private void ConfigureBuilder(ExpressionBuilder builder, GraphNode selectedNode, string arguments)
        {
            if (string.IsNullOrEmpty(arguments)) return;
            // TODO: This special case for binary operator operands should be avoided in the future
            if (builder is BinaryOperatorBuilder binaryOperator && selectedNode != null)
            {
                if (((Node<ExpressionBuilder, ExpressionBuilderArgument>)selectedNode.Tag).Value is InspectBuilder inputBuilder &&
                    inputBuilder.ObservableType != null)
                {
                    binaryOperator.Build(Expression.Parameter(typeof(IObservable<>).MakeGenericType(inputBuilder.ObservableType)));
                }
            }

            var workflowElement = ExpressionBuilder.GetWorkflowElement(builder);
            var defaultProperty = TypeDescriptor.GetDefaultProperty(workflowElement);
            if (defaultProperty != null &&
                !defaultProperty.IsReadOnly &&
                defaultProperty.Converter != null &&
                defaultProperty.Converter.CanConvertFrom(typeof(string)))
            {
                try
                {
                    var context = new TypeDescriptorContext(workflowElement, defaultProperty, serviceProvider);
                    var propertyValue = defaultProperty.Converter.ConvertFromString(context, arguments);
                    defaultProperty.SetValue(workflowElement, propertyValue);
                }
                catch (Exception ex)
                {
                    throw new SystemException(ex.Message, ex);
                }
            }
        }

        public void CreateGraphNode(ExpressionBuilder builder, GraphNode selectedNode, CreateGraphNodeType nodeType, bool branch, bool validate = true)
        {
            var selection = selectedNode != null ? new[] { selectedNode } : Enumerable.Empty<GraphNode>();
            var commands = GetCreateGraphNodeCommands(builder, selection, nodeType, branch, validate);
            commandExecutor.Execute(
            () =>
            {
                commands.Item1.Command();
                commands.Item2.Command();
            },
            () =>
            {
                commands.Item1.Undo();
                commands.Item2.Undo();
            });
        }

        Tuple<GraphCommand, GraphCommand> GetCreateGraphNodeCommands(
            ExpressionBuilder builder,
            IEnumerable<GraphNode> selectedNodes,
            CreateGraphNodeType nodeType,
            bool branch,
            bool validate = true)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            var workflow = this.Workflow;
            if (builder is WorkflowInputBuilder workflowInput)
            {
                workflowInput.Index = workflow.Count(node => ExpressionBuilder.Unwrap(node.Value) is WorkflowInputBuilder);
            }

            var inspectBuilder = builder.AsInspectBuilder();
            var inspectNode = new Node<ExpressionBuilder, ExpressionBuilderArgument>(inspectBuilder);
            var inspectParameter = new ExpressionBuilderArgument();
            Action addNode = () => { AddWorkflowNode(workflow, inspectNode); };
            Action removeNode = () => { RemoveWorkflowNode(workflow, inspectNode); };
            builder = inspectBuilder.Builder;

            var targetNodes = selectedNodes.Select(node => GetGraphNodeTag(workflow, node)).ToArray();
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);

            if (builder is WorkflowExpressionBuilder workflowBuilder && validate)
            {
                // Estimate number of inputs to the nested node
                var inputCount = workflowBuilder.ArgumentRange.LowerBound;
                if (nodeType == CreateGraphNodeType.Successor) inputCount = Math.Max(inputCount, targetNodes.Length);
                else inputCount = Math.Max(inputCount, targetNodes.Sum(node => workflow.PredecessorEdges(node).Count()));

                // Limit number of inputs depending on nested operator argument range
                if (!(workflowBuilder is GroupWorkflowBuilder || workflowBuilder is NestedWorkflowBuilder))
                {
                    inputCount = Math.Min(inputCount, workflowBuilder.ArgumentRange.UpperBound);
                }

                for (int i = 0; i < inputCount; i++)
                {
                    var nestedInput = new WorkflowInputBuilder { Index = i };
                    var nestedInputInspectBuilder = new InspectBuilder(nestedInput);
                    var nestedInputNode = workflowBuilder.Workflow.Add(nestedInputInspectBuilder);
                    if (inputCount == 1)
                    {
                        var nestedOutput = new WorkflowOutputBuilder();
                        var nestedOutputInspectBuilder = new InspectBuilder(nestedOutput);
                        var nestedOutputNode = workflowBuilder.Workflow.Add(nestedOutputInspectBuilder);
                        workflowBuilder.Workflow.AddEdge(nestedInputNode, nestedOutputNode, new ExpressionBuilderArgument());
                    }
                }
            }

            var validateInsert = validate && !(nodeType == CreateGraphNodeType.Predecessor && builder.IsBuildDependency());
            if (validate && !branch && targetNodes.Length > 1 &&
               ((nodeType == CreateGraphNodeType.Successor && targetNodes.Skip(1).Any(node => targetNodes[0].DepthFirstSearch().Contains(node))) ||
                (nodeType == CreateGraphNodeType.Predecessor && targetNodes.Skip(1).Any(node => node.DepthFirstSearch().Contains(targetNodes[0])))))
            {
                throw new InvalidOperationException(Resources.InsertValidation_Error);
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var updateSelectedNode = CreateUpdateSelectionDelegate(builder);
            var insertCommands = GetInsertGraphNodeCommands(inspectNode, inspectNode, targetNodes, nodeType, branch, validateInsert);
            var addConnection = insertCommands.Command;
            var removeConnection = insertCommands.Undo;

            GraphCommand createNode;
            createNode.Command = () =>
            {
                addNode();
                addConnection();
            };
            createNode.Undo = () =>
            {
                removeConnection();
                removeNode();
            };

            GraphCommand updateLayout;
            if (validate)
            {
                updateLayout.Command = () =>
                {
                    updateGraphLayout();
                    updateSelectedNode();
                };
                updateLayout.Undo = () =>
                {
                    updateGraphLayout();
                    restoreSelectedNodes();
                };
            }
            else
            {
                updateLayout.Command = EmptyAction;
                updateLayout.Undo = EmptyAction;
            }

            return Tuple.Create(createNode, updateLayout);
        }

        public void InsertGraphElements(ExpressionBuilderGraph elements, CreateGraphNodeType nodeType, bool branch)
        {
            if (elements == null)
            {
                throw new ArgumentNullException("elements");
            }

            var selectedNodes = graphView.SelectedNodes.ToArray();
            var updateSelectedNodes = CreateUpdateSelectionDelegate(elements.Sinks().FirstOrDefault());
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();

            commandExecutor.Execute(EmptyAction, updateGraphLayout + restoreSelectedNodes);
            InsertGraphElements(elements, selectedNodes, nodeType, branch, EmptyAction, EmptyAction);
            commandExecutor.Execute(updateGraphLayout + updateSelectedNodes, EmptyAction);
        }

        private void InsertGraphElements(
            ExpressionBuilderGraph elements,
            GraphNode[] selectedNodes,
            CreateGraphNodeType nodeType,
            bool branch,
            Action addConnection,
            Action removeConnection)
        {
            if (selectedNodes.Length > 0)
            {
                var targetNodes = selectedNodes.Select(node => GetGraphNodeTag(Workflow, node));
                var source = elements.Sources().FirstOrDefault();
                var sink = elements.Sinks().FirstOrDefault();
                if (source != null && sink != null)
                {
                    var insertCommands = GetInsertGraphNodeCommands(source, sink, targetNodes, nodeType, branch);
                    addConnection += insertCommands.Command;
                    removeConnection += insertCommands.Undo;
                }
            }

            commandExecutor.Execute(
            () =>
            {
                foreach (var node in elements)
                {
                    AddWorkflowNode(Workflow, node);
                }
                addConnection();
            },
            () =>
            {
                removeConnection();
                foreach (var node in elements.TopologicalSort())
                {
                    RemoveWorkflowNode(Workflow, node);
                }
            });
            ReplaceExternalizedMappings(nodeType, selectedNodes);
        }

        void DeleteGraphNode(GraphNode node)
        {
            DeleteGraphNode(node, true);
        }

        void DeleteGraphNode(GraphNode node, bool replaceEdges)
        {
            var workflowNode = GetGraphNodeTag(Workflow, node);
            DeleteGraphNode(workflowNode, replaceEdges);
        }

        void DeleteGraphNode(Node<ExpressionBuilder, ExpressionBuilderArgument> workflowNode, bool replaceEdges)
        {
            var workflow = this.Workflow;
            if (workflowNode == null)
            {
                throw new ArgumentNullException(nameof(workflowNode));
            }

            var addEdge = EmptyAction;
            var removeEdge = EmptyAction;

            var predecessorEdges = workflow.PredecessorEdges(workflowNode).ToArray();
            var siblingEdgesAfter = (from edge in workflowNode.Successors
                                     from siblingEdge in workflow.PredecessorEdges(edge.Target)
                                     where siblingEdge.Item2.Label.Index.CompareTo(edge.Label.Index) > 0
                                     select siblingEdge.Item2)
                                     .ToArray();

            var simplePredecessor = predecessorEdges.Length == 1;
            var simpleSuccessor = (workflowNode.Successors.Count == 1 && workflow.Predecessors(workflowNode.Successors[0].Target).Count() == 1);
            replaceEdges &= simplePredecessor || simpleSuccessor;
            if (replaceEdges)
            {
                var replacedEdges = (from predecessor in predecessorEdges
                                     from successor in workflowNode.Successors
                                     where !workflow.Successors(predecessor.Item1).Contains(successor.Target)
                                     select new
                                     {
                                         predecessor = predecessor.Item1,
                                         edgeIndex = predecessor.Item3,
                                         edge = simplePredecessor
                                            ? successor
                                            : Edge.Create(successor.Target, predecessor.Item2.Label)
                                     })
                                     .Reverse()
                                     .ToArray();

                addEdge = () =>
                {
                    Array.ForEach(replacedEdges, replacedEdge =>
                    {
                        if (simplePredecessor) workflow.InsertEdge(replacedEdge.predecessor, replacedEdge.edgeIndex, replacedEdge.edge);
                        else workflow.SetEdge(replacedEdge.predecessor, replacedEdge.edgeIndex, replacedEdge.edge);
                    });
                };

                removeEdge = () =>
                {
                    Array.ForEach(replacedEdges, replacedEdge =>
                    {
                        workflow.RemoveEdge(replacedEdge.predecessor, replacedEdge.edge);
                    });
                };
            }

            Action removeNode = () =>
            {
                RemoveWorkflowNode(workflow, workflowNode);
                if (!replaceEdges)
                {
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index--;
                    }
                }
            };

            Action addNode = () =>
            {
                AddWorkflowNode(workflow, workflowNode);
                foreach (var edge in predecessorEdges)
                {
                    edge.Item1.Successors.Insert(edge.Item3, edge.Item2);
                }

                if (!replaceEdges)
                {
                    foreach (var sibling in siblingEdgesAfter)
                    {
                        sibling.Label.Index++;
                    }
                }
            };

            commandExecutor.Execute(() =>
            {
                addEdge();
                removeNode();
            },
            () =>
            {
                addNode();
                removeEdge();
            });

            var builder = ExpressionBuilder.Unwrap(workflowNode.Value);
            var disableBuilder = builder as DisableBuilder;
            var workflowExpressionBuilder = (disableBuilder != null ? disableBuilder.Builder : builder) as IWorkflowExpressionBuilder;
            if (workflowExpressionBuilder != null)
            {
                closeWorkflowEditor.OnNext(workflowExpressionBuilder);
            }
        }

        public void DeleteGraphNodes(IEnumerable<GraphNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (!nodes.Any()) return;
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, updateGraphLayout);
            foreach (var node in nodes)
            {
                DeleteGraphNode(node);
            }

            commandExecutor.Execute(updateGraphLayout, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        public void MoveGraphNodes(IEnumerable<GraphNode> nodes, GraphNode target, CreateGraphNodeType nodeType, bool branch)
        {
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            updateGraphLayout += CreateUpdateSelectionDelegate(nodes);
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, updateGraphLayout);

            var elements = nodes.ToWorkflowBuilder().Workflow.ToInspectableGraph();
            var sortedNodes = nodes.OrderBy(n => n.Value, elements.Comparer).ToList();
            var buildDependencies = (from item in sortedNodes.Zip(elements, (node, element) => new { node, element })
                                     from predecessor in Workflow.PredecessorEdges(GetGraphNodeTag(Workflow, item.node))
                                     where predecessor.Item1.Value.IsBuildDependency() && !elements.Any(node => node.Value == item.node.Value)
                                     orderby predecessor.Item3
                                     select new { predecessor, edge = Edge.Create(item.element, predecessor.Item2.Label) }).ToArray();
            commandExecutor.Execute(
                () => Array.ForEach(buildDependencies, dependency => Workflow.RemoveEdge(dependency.predecessor.Item1, dependency.predecessor.Item2)),
                () => Array.ForEach(buildDependencies, dependency => Workflow.InsertEdge(dependency.predecessor.Item1, dependency.predecessor.Item3, dependency.predecessor.Item2)));

            foreach (var node in nodes)
            {
                DeleteGraphNode(node);
            }

            Action addConnection = () => Array.ForEach(buildDependencies, dependency => Workflow.AddEdge(dependency.predecessor.Item1, dependency.edge));
            Action removeConnection = () => Array.ForEach(buildDependencies, dependency => Workflow.RemoveEdge(dependency.predecessor.Item1, dependency.edge));
            InsertGraphElements(elements, new[] { target }, nodeType, branch, addConnection, removeConnection);
            commandExecutor.Execute(updateGraphLayout, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        private void ReplaceNode(GraphNode node, ExpressionBuilder builder)
        {
            CreateGraphNode(builder, node, CreateGraphNodeType.Successor, branch: false, validate: false);
            DeleteGraphNode(node);
        }

        private bool CanGroup(IEnumerable<GraphNode> nodes, WorkflowBuilder groupBuilder)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            var workflow = this.Workflow;
            var selectedNodes = nodes.Select(node => (Node<ExpressionBuilder, ExpressionBuilderArgument>)node.Tag);
            return !(from node in groupBuilder.Workflow.Sources()
                     let source = FindWorkflowValue(workflow, node.Value)
                     let connectivity = node.DepthFirstSearch()
                                            .Select(successor => FindWorkflowValue(workflow, successor.Value))
                                            .ToArray()
                     from successor in source.DepthFirstSearch()
                     where !connectivity.Contains(successor) && selectedNodes.Contains(successor)
                     select successor).Any();
        }

        public void GroupGraphNodes(IEnumerable<GraphNode> nodes)
        {
            GroupGraphNodes(nodes, graph => new GroupWorkflowBuilder(graph));
        }

        public void GroupGraphNodes(IEnumerable<GraphNode> nodes, string typeName)
        {
            GroupGraphNodes(nodes, graph => CreateWorkflowBuilder(typeName, graph));
        }

        private void GroupGraphNodes(IEnumerable<GraphNode> nodes, Func<ExpressionBuilderGraph, WorkflowExpressionBuilder> groupFactory)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (!nodes.Any()) return;
            var workflow = this.Workflow;
            GraphNode replacementNode = null;
            var nodeType = CreateGraphNodeType.Successor;
            var workflowBuilder = nodes.ToWorkflowBuilder(recurse: false);
            if (!CanGroup(nodes, workflowBuilder))
            {
                error.OnNext(new InvalidOperationException(Resources.GroupBrokenBranches_Error));
                return;
            }

            var inputIndex = 0;
            var predecessors = (from node in workflow
                                where !node.Value.IsBuildDependency()
                                let graphNode = FindGraphNode(node.Value)
                                where graphNode != null
                                orderby graphNode.Layer descending, graphNode.LayerIndex
                                let unwrapNode = ExpressionBuilder.Unwrap(node.Value)
                                where !workflowBuilder.Workflow.Any(n => n.Value == unwrapNode)
                                from successor in node.Successors
                                let unwrapSuccessor = ExpressionBuilder.Unwrap(successor.Target.Value)
                                let target = workflowBuilder.Workflow.FirstOrDefault(n => n.Value == unwrapSuccessor)
                                where target != null
                                group new { successor.Label.Index, target } by node).ToArray();
            var successors = (from node in workflowBuilder.Workflow
                              let workflowNode = workflow.Single(n => ExpressionBuilder.Unwrap(n.Value) == node.Value)
                              from successor in workflowNode.Successors
                              let unwrapSuccessor = ExpressionBuilder.Unwrap(successor.Target.Value)
                              where !workflowBuilder.Workflow.Any(n => n.Value == unwrapSuccessor)
                              group new { successor, node, workflowNode } by successor.Target).ToArray();

            foreach (var predecessor in predecessors)
            {
                var workflowInput = new WorkflowInputBuilder { Index = inputIndex++ };
                var inputNode = workflowBuilder.Workflow.Add(workflowInput);
                foreach (var edge in predecessor)
                {
                    workflowBuilder.Workflow.AddEdge(inputNode, edge.target, new ExpressionBuilderArgument(edge.Index));
                }
            }

            var sinks = workflowBuilder.Workflow.Sinks().ToArray();
            if (sinks.Length == 1 && !(sinks[0].Value is WorkflowOutputBuilder))
            {
                var sink = sinks.First();
                var workflowOutput = new WorkflowOutputBuilder();
                var outputNode = workflowBuilder.Workflow.Add(workflowOutput);
                workflowBuilder.Workflow.AddEdge(sink, outputNode, new ExpressionBuilderArgument());

                var sinkNode = graphView.Nodes.LayeredNodes().Single(node => GetGraphNodeBuilder(node) == sink.Value);
                if (sinkNode.Successors.Count() > 0)
                {
                    replacementNode = sinkNode;
                    nodeType = CreateGraphNodeType.Predecessor;
                }
            }

            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var workflowExpressionBuilder = groupFactory(workflowBuilder.Workflow.ToInspectableGraph(recurse: false));
            var updateSelectedNode = CreateUpdateSelectionDelegate(workflowExpressionBuilder);

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, updateGraphLayout);
            foreach (var node in nodes.Where(n => n != replacementNode))
            {
                DeleteGraphNode(node, replaceEdges: false);
            }

            CreateGraphNode(workflowExpressionBuilder,
                            replacementNode,
                            nodeType,
                            branch: false,
                            validate: false);

            // Connect grouped node predecessors and successors
            var predecessorEdges = new List<Tuple<Node<ExpressionBuilder, ExpressionBuilderArgument>, Edge<ExpressionBuilder, ExpressionBuilderArgument>>>();
            var successorEdges = new List<Tuple<Node<ExpressionBuilder, ExpressionBuilderArgument>, Edge<ExpressionBuilder, ExpressionBuilderArgument>>>();
            commandExecutor.Execute(() =>
            {
                var linkIndex = 0;
                var groupNode = workflow.Single(node => ExpressionBuilder.Unwrap(node.Value) == workflowExpressionBuilder);
                foreach (var predecessor in predecessors)
                {
                    var predecessorEdge = predecessor.Key.Successors
                        .FirstOrDefault(edge => edge.Target == groupNode && edge.Label.Index == linkIndex);
                    if (predecessorEdge == null)
                    {
                        var edge = workflow.AddEdge(predecessor.Key, groupNode, new ExpressionBuilderArgument { Index = linkIndex });
                        predecessorEdges.Add(Tuple.Create(predecessor.Key, edge));
                    }

                    linkIndex++;
                }

                foreach (var successor in successors)
                {
                    linkIndex = workflow.PredecessorEdges(successor.Key).Count();
                    var sinkNode = replacementNode == null ? groupNode : GetGraphNodeTag(workflow, replacementNode);
                    var successorEdge = sinkNode.Successors.FirstOrDefault(edge => edge.Target == successor.Key);
                    if (successorEdge == null)
                    {
                        var edge = workflow.AddEdge(sinkNode, successor.Key, new ExpressionBuilderArgument { Index = linkIndex });
                        successorEdges.Add(Tuple.Create(groupNode, edge));
                    }
                }
            },
            () =>
            {
                foreach (var edge in predecessorEdges)
                {
                    workflow.RemoveEdge(edge.Item1, edge.Item2);
                }

                foreach (var edge in successorEdges)
                {
                    workflow.RemoveEdge(edge.Item1, edge.Item2);
                }
            });

            if (replacementNode != null) DeleteGraphNode(replacementNode);
            commandExecutor.Execute(() =>
            {
                updateGraphLayout();
                updateSelectedNode();
            },
            EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        public void ReplaceGroupNode(GraphNode node, string typeName)
        {
            if (!(GetGraphNodeBuilder(node) is WorkflowExpressionBuilder workflowBuilder))
            {
                throw new ArgumentException(Resources.InvalidReplaceGroupNode_Error, nameof(node));
            }

            if (workflowBuilder.GetType().AssemblyQualifiedName != typeName)
            {
                var builder = CreateWorkflowBuilder(typeName, workflowBuilder.Workflow);
                builder.Name = workflowBuilder.Name;
                builder.Description = workflowBuilder.Description;
                ReplaceGraphNode(node, builder);
            }
        }

        public void ReplaceGraphNode(GraphNode node, string typeName, ElementCategory elementCategory, string arguments)
        {
            var selectedBuilder = GetGraphNodeBuilder(node);
            var selectedBuilderType = selectedBuilder.GetType();
            if (selectedBuilderType.AssemblyQualifiedName == typeName)
            {
                return;
            }

            var allowGenericSource = elementCategory == ~ElementCategory.Combinator;
            if (allowGenericSource && selectedBuilder is SubjectExpressionBuilder &&
                selectedBuilderType.IsGenericType)
            {
                typeName = MakeGenericType(typeName, node, out elementCategory);
            }

            ExpressionBuilder builder;
            if (selectedBuilder is WorkflowExpressionBuilder workflowBuilder &&
                typeof(WorkflowExpressionBuilder).IsAssignableFrom(Type.GetType(typeName)))
            {
                var replaceBuilder = CreateWorkflowBuilder(typeName, workflowBuilder.Workflow);
                replaceBuilder.Name = workflowBuilder.Name;
                replaceBuilder.Description = workflowBuilder.Description;
                builder = replaceBuilder;
            }
            else
            {
                var group = node.Category == ElementCategory.Sink;
                builder = CreateBuilder(typeName, elementCategory, group);
                if (selectedBuilder is INamedElement namedBuilder &&
                   (namedBuilder is SubjectExpressionBuilder ||
                    namedBuilder is SubscribeSubjectBuilder ||
                    namedBuilder is MulticastSubjectBuilder))
                {
                    if (builder is SubjectExpressionBuilder subjectBuilder)
                    {
                        subjectBuilder.Name = namedBuilder.Name;
                    }
                    else if (builder is SubscribeSubjectBuilder subscribeBuilder &&
                        string.IsNullOrEmpty(subscribeBuilder.Name))
                    {
                        subscribeBuilder.Name = namedBuilder.Name;
                    }
                    else if (builder is MulticastSubjectBuilder multicastBuilder &&
                        string.IsNullOrEmpty(multicastBuilder.Name))
                    {
                        multicastBuilder.Name = namedBuilder.Name;
                    }
                }
            }
            ReplaceGraphNode(node, builder);
            ConfigureBuilder(builder, null, arguments);
        }

        public void ReplaceGraphNode(GraphNode node, ExpressionBuilder builder)
        {
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var selectCreatedNode = CreateUpdateSelectionDelegate(builder);
            var selectDeletedNode = CreateUpdateSelectionDelegate(node);

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, () =>
            {
                updateGraphLayout();
                selectDeletedNode();
            });
            ReplaceNode(node, builder);
            commandExecutor.Execute(() =>
            {
                updateGraphLayout();
                selectCreatedNode();
            },
            EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        private void UpdateGraphNodes(IEnumerable<GraphNode> nodes, Action<GraphNode> action)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (!nodes.Any()) return;
            var selectedNodes = nodes.ToArray();
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var updateSelectedNode = CreateUpdateSelectionDelegate();
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, restoreSelectedNodes);
            commandExecutor.Execute(updateSelectedNode, updateGraphLayout);
            foreach (var node in selectedNodes)
            {
                action(node);
            }

            commandExecutor.Execute(updateGraphLayout, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        public void UngroupGraphNodes(IEnumerable<GraphNode> nodes)
        {
            UpdateGraphNodes(nodes, UngroupOrReplaceGraphNode);
        }

        private void UngroupOrReplaceGraphNode(GraphNode node)
        {
            var selectedNodeBuilder = node != null ? GetGraphNodeBuilder(node) : null;
            if (selectedNodeBuilder is IncludeWorkflowBuilder includeBuilder)
            {
                var groupBuilder = new GroupWorkflowBuilder(includeBuilder.Workflow);
                groupBuilder.Name = includeBuilder.Name;
                groupBuilder.Description = includeBuilder.Description;
                ReplaceNode(node, groupBuilder);
            }
            else UngroupGraphNode(node);
        }

        private void UngroupGraphNode(GraphNode node)
        {
            var workflow = this.Workflow;
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var workflowNode = GetGraphNodeTag(workflow, node);
            if (!(ExpressionBuilder.Unwrap(workflowNode.Value) is WorkflowExpressionBuilder workflowBuilder))
            {
                return;
            }

            var predecessors = workflow.PredecessorEdges(workflowNode).OrderBy(edge => edge.Item2.Label.Index).Select(xs => xs.Item1.Value).ToArray();
            var successors = workflowNode.Successors.Select(xs => xs.Target.Value).ToArray();
            var groupWorkflow = new ExpressionBuilderGraph();
            groupWorkflow.AddDescriptor(workflowBuilder.Workflow.ToDescriptor());

            var groupSources = (from n in groupWorkflow
                                let source = ExpressionBuilder.Unwrap(n.Value) as WorkflowInputBuilder
                                where source != null
                                orderby source.Index ascending
                                select n).ToArray();
            var groupSinks = (from n in groupWorkflow
                              let sink = ExpressionBuilder.Unwrap(n.Value) as WorkflowOutputBuilder
                              where sink != null
                              select n).ToArray();
            var groupOutputs = groupSinks.Take(1)
                .Select(groupWorkflow.PredecessorEdges)
                .SelectMany(edges => edges.OrderBy(edge => edge.Item2.Label.Index))
                .ToArray();
            foreach (var terminal in groupSources.Concat(groupSinks))
            {
                groupWorkflow.Remove(terminal);
            }

            DeleteGraphNode(node, replaceEdges: false);
            InsertGraphElements(groupWorkflow, CreateGraphNodeType.Successor, false);

            // Connect incoming nodes to internal targets
            var mainSink = groupSinks.FirstOrDefault();
            var inputConnections = predecessors
                .Select(xs => FindGraphNode(xs))
                .Zip(groupSources, (xs, ys) =>
                    ys.Successors.SelectMany(zs => zs.Target != mainSink
                                     ? Enumerable.Repeat(Tuple.Create(xs, FindGraphNode(zs.Target.Value)), 1)
                                     : successors.Select(ss => Tuple.Create(xs, FindGraphNode(ss)))));
            foreach (var input in inputConnections.SelectMany(xs => xs))
            {
                ConnectInternalNodes(input.Item1, input.Item2);
            }

            // Connect output sources to external targets
            var outputConnections = groupOutputs
                .Select(edge => FindGraphNode(edge.Item1.Value))
                .Where(xs => xs != null)
                .SelectMany(xs => successors.Select(edge =>
                    Tuple.Create(xs, FindGraphNode(edge))));
            foreach (var output in outputConnections)
            {
                ConnectInternalNodes(output.Item1, output.Item2);
            }
        }

        void DisableGraphNode(GraphNode node)
        {
            var builder = GetGraphNodeBuilder(node);
            var disableBuilder = builder as DisableBuilder;
            if (builder != null && disableBuilder == null)
            {
                builder = new DisableBuilder(builder);
                ReplaceNode(node, builder);
            }
        }

        void EnableGraphNode(GraphNode node)
        {
            var builder = GetGraphNodeBuilder(node);
            if (builder is DisableBuilder disableBuilder)
            {
                builder = disableBuilder.Builder;
                ReplaceNode(node, builder);
            }
        }

        public void DisableGraphNodes(IEnumerable<GraphNode> nodes)
        {
            UpdateGraphNodes(nodes, DisableGraphNode);
        }

        public void EnableGraphNodes(IEnumerable<GraphNode> nodes)
        {
            UpdateGraphNodes(nodes, EnableGraphNode);
        }

        public void RenameSubject(SubjectDefinition definition, string newName)
        {
            if (definition == null)
            {
                throw new ArgumentNullException(nameof(definition));
            }

            if (definition.IsReadOnly)
            {
                throw new ArgumentException(Resources.ReadOnlySubjectDefinition_Error, nameof(definition));
            }

            var currentName = definition.Subject.Name;
            var subscribeDependents = new List<SubscribeSubjectBuilder>();
            var multicastDependents = new List<MulticastSubjectBuilder>();
            foreach (var dependent in definition.GetDependentExpressions()
                                                .SelectMany(context => context)
                                                .Where(element => element.Builder is INamedElement namedElement &&
                                                    namedElement.Name == definition.Subject.Name))
            {
                if (dependent.IsReadOnly) continue;
                else if (dependent.Builder is SubscribeSubjectBuilder subscribeSubject)
                {
                    subscribeDependents.Add(subscribeSubject);
                }
                else if (dependent.Builder is MulticastSubjectBuilder multicastSubject)
                {
                    multicastDependents.Add(multicastSubject);
                }
            }

            var invalidateGraphLayout = CreateInvalidateGraphLayoutDelegate();
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, invalidateGraphLayout);
            commandExecutor.Execute(() =>
            {
                definition.Subject.Name = newName;
                foreach (var dependent in subscribeDependents)
                {
                    dependent.Name = newName;
                }

                foreach (var dependent in multicastDependents)
                {
                    dependent.Name = newName;
                }
            },
            () =>
            {
                definition.Subject.Name = currentName;
                foreach (var dependent in subscribeDependents)
                {
                    dependent.Name = currentName;
                }

                foreach (var dependent in multicastDependents)
                {
                    dependent.Name = currentName;
                }
            });
            commandExecutor.Execute(invalidateGraphLayout, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        public GraphNode FindGraphNode(ExpressionBuilder value)
        {
            return graphView.Nodes.SelectMany(layer => layer).FirstOrDefault(n => n.Value == value);
        }
    }
    
    enum CreateGraphNodeType
    {
        Successor,
        Predecessor
    }

    struct GraphCommand
    {
        public Action Command;
        public Action Undo;

        public GraphCommand(Action command, Action undo)
        {
            Command = command;
            Undo = undo;
        }
    }
}
