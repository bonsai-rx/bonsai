using Bonsai.Dag;
using Bonsai.Design;
using Bonsai.Editor.Properties;
using Bonsai.Expressions;
using Bonsai.Reactive;
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
        readonly Subject<bool> invalidateLayout;
        readonly Subject<WorkflowEditorPath> workflowPathChanged;
        readonly Subject<IEnumerable<ExpressionBuilder>> updateSelection;
        WorkflowEditorPath workflowPath;

        public WorkflowEditor(IServiceProvider provider, IGraphView view)
        {
            graphView = view ?? throw new ArgumentNullException(nameof(view));
            serviceProvider = provider ?? throw new ArgumentNullException(nameof(provider));
            commandExecutor = (CommandExecutor)provider.GetService(typeof(CommandExecutor));
            error = new Subject<Exception>();
            updateLayout = new Subject<bool>();
            invalidateLayout = new Subject<bool>();
            workflowPathChanged = new Subject<WorkflowEditorPath>();
            updateSelection = new Subject<IEnumerable<ExpressionBuilder>>();
            ResetNavigation();
        }

        public ExpressionBuilderGraph Workflow { get; private set; }

        public WorkflowPathFlags WorkflowPathFlags { get; private set; }

        public WorkflowEditorPath WorkflowPath
        {
            get { return workflowPath; }
            private set
            {
                workflowPath = value;
                var workflowBuilder = (WorkflowBuilder)serviceProvider.GetService(typeof(WorkflowBuilder));
                if (workflowPath != null)
                {
                    var builder = workflowPath.Resolve(workflowBuilder, out WorkflowPathFlags pathFlags);
                    if (ExpressionBuilder.GetWorkflowElement(builder) is not IWorkflowExpressionBuilder workflowExpressionBuilder)
                    {
                        throw new ArgumentException(Resources.InvalidWorkflowPath_Error, nameof(value));
                    }

                    Workflow = workflowExpressionBuilder.Workflow;
                    WorkflowPathFlags = pathFlags;
                }
                else
                {
                    Workflow = workflowBuilder.Workflow;
                    WorkflowPathFlags = WorkflowPathFlags.None;
                }
                updateLayout.OnNext(false);
                workflowPathChanged.OnNext(workflowPath);
            }
        }

        public IObservable<Exception> Error => error;

        public IObservable<bool> UpdateLayout => updateLayout;

        public IObservable<bool> InvalidateLayout => invalidateLayout;

        public IObservable<WorkflowEditorPath> WorkflowPathChanged => workflowPathChanged;

        public IObservable<IEnumerable<ExpressionBuilder>> UpdateSelection => updateSelection;

        private static Node<ExpressionBuilder, ExpressionBuilderArgument> FindWorkflowValue(ExpressionBuilderGraph workflow, ExpressionBuilder value)
        {
            return workflow.Single(n => ExpressionBuilder.Unwrap(n.Value) == value);
        }

        private static Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, int> RangeIndices(
            ExpressionBuilderGraph workflow,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> source)
        {
            var sourceNodes = new HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>>(source);
            var nodeMap = new Dictionary<Node<ExpressionBuilder, ExpressionBuilderArgument>, int>(sourceNodes.Count);
            for (int i = 0; i < workflow.Count; i++)
            {
                var node = workflow[i];
                if (sourceNodes.Contains(node))
                {
                    nodeMap[node] = i;
                }
            }

            return nodeMap;
        }

        private int IndexOfComponentNode(ExpressionBuilderGraph workflow, ExpressionBuilderGraph component)
        {
            for (int i = 0; i < workflow.Count; i++)
            {
                if (component.Contains(workflow[i]))
                {
                    return i;
                }
            }

            return -1;
        }

        private int LastIndexOfComponentNode(ExpressionBuilderGraph workflow, ExpressionBuilderGraph component)
        {
            var insertIndex = 0;
            for (int i = 0; i < workflow.Count; i++)
            {
                if (component.Contains(workflow[i]))
                {
                    insertIndex = i;
                }
            }

            return insertIndex;
        }

        private int GetInsertIndex(
            ExpressionBuilderGraph workflow,
            ExpressionBuilder builder,
            GraphNode target,
            CreateGraphNodeType nodeType,
            bool branch)
        {
            var targetNode = target != null ? GetGraphNodeTag(workflow, target) : null;
            return GetInsertIndex(workflow, builder, targetNode, nodeType, branch);
        }

        private int GetInsertIndex(
            ExpressionBuilderGraph workflow,
            ExpressionBuilder builder,
            Node<ExpressionBuilder, ExpressionBuilderArgument> target,
            CreateGraphNodeType nodeType,
            bool branch)
        {
            var allowConnection = builder != null && GetBuilderMaxConnectionCount(builder) > 0;
            var forwardBranch = branch && nodeType == CreateGraphNodeType.Successor;
            var insertComponent = forwardBranch || graphView.SelectedNodes.Count() == 0 || !allowConnection;

            if (insertComponent && target != null)
            {
                if (forwardBranch && allowConnection)
                {
                    var lastSuccessor = target.DepthFirstSearch().Last();
                    return workflow.IndexOf(lastSuccessor) + 1;
                }

                var components = workflow.FindConnectedComponents();
                var targetComponent = components.First(component => component.Contains(target));
                return nodeType == CreateGraphNodeType.Successor
                    ? LastIndexOfComponentNode(workflow, targetComponent) + 1
                    : IndexOfComponentNode(workflow, targetComponent);
            }

            return GetInsertIndex(workflow, target, nodeType);
        }

        private int GetInsertIndex(ExpressionBuilderGraph workflow, GraphNode target, CreateGraphNodeType nodeType)
        {
            var insertNode = GetGraphNodeTag(target);
            return GetInsertIndex(workflow, insertNode, nodeType);
        }

        private int GetInsertIndex(
            ExpressionBuilderGraph workflow,
            Node<ExpressionBuilder, ExpressionBuilderArgument> target,
            CreateGraphNodeType nodeType)
        {
            if (target == null) return workflow.Count;
            var insertOffset = nodeType == CreateGraphNodeType.Successor ? 1 : 0;
            return workflow.IndexOf(target) + insertOffset;
        }

        private void AddWorkflowInput(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
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
            }
        }

        private void RemoveWorkflowInput(ExpressionBuilderGraph workflow, Node<ExpressionBuilder, ExpressionBuilderArgument> node)
        {
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
            }
        }

        private void SortWorkflow()
        {
            SortWorkflow(Workflow);
        }

        private void SortWorkflow(ExpressionBuilderGraph workflow)
        {
            workflow.InsertRange(0, workflow.TopologicalSort());
        }

        private GraphCommand GetSimpleSort()
        {
            return new GraphCommand(SortWorkflow, SortWorkflow);
        }

        private GraphCommand GetReversibleSort()
        {
            var rootOrder = Workflow.Sinks().ToArray();
            return new GraphCommand(
                SortWorkflow,
                () =>
                {
                    Workflow.InsertRange(0, rootOrder);
                    SortWorkflow();
                });
        }

        private Action CreateUpdateGraphLayoutDelegate()
        {
            return () => updateLayout.OnNext(true);
        }

        private Action CreateInvalidateGraphLayoutDelegate()
        {
            return () => invalidateLayout.OnNext(true);
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

        private static Node<ExpressionBuilder, ExpressionBuilderArgument> GetGraphNodeTag(GraphNode node)
        {
            return (Node<ExpressionBuilder, ExpressionBuilderArgument>)node?.Tag;
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

        static int GetBuilderMaxConnectionCount(ExpressionBuilder builder)
        {
            var mappingBuilder = ExpressionBuilder.Unwrap(builder) as ExternalizedMappingBuilder;
            return mappingBuilder != null ? 1 : builder.ArgumentRange.UpperBound;
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
                    var maxConnectionCount = GetBuilderMaxConnectionCount(node.Value);
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
                                if (!predecessorNode.Value.IsBuildDependency())
                                {
                                    addConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, sourceNode, predecessorEdge.Label); };
                                    removeConnection += () => { workflow.SetEdge(predecessorNode, edgeIndex, predecessorEdge); };
                                }
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
            else if (!validate || sourceNode.Value.ArgumentRange.UpperBound > 0 || targetNodes.All(node => node.Value.IsBuildDependency()))
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

        private void ConnectInternalNodes(
            Node<ExpressionBuilder, ExpressionBuilderArgument> source,
            Node<ExpressionBuilder, ExpressionBuilderArgument> target)
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
            var target = GetGraphNodeTag(Workflow, graphViewTarget);
            var sourceNodes = graphViewSources.Select(node => GetGraphNodeTag(Workflow, node)).ToArray();
            ConnectGraphNodes(sourceNodes, target, validate);
        }

        private void ConnectGraphNodes(
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> sourceNodes,
            Node<ExpressionBuilder, ExpressionBuilderArgument> target,
            bool validate)
        {
            var workflow = this.Workflow;
            var addConnection = EmptyAction;
            var removeConnection = EmptyAction;
            var sortedPredecessors = workflow.PredecessorEdges(target)
                .Select(edge => edge.Item2.Label.Index)
                .OrderBy(idx => idx).ToArray();

            var offset = 0;
            var connectionIndex = 0;
            var sinkNodes = default(List<Node<ExpressionBuilder, ExpressionBuilderArgument>>);
            foreach (var source in sourceNodes)
            {
                if (source.Successors.Count == 0)
                {
                    sinkNodes ??= new();
                    sinkNodes.Add(source);
                }

                FindNextIndex(ref connectionIndex, ref offset, sortedPredecessors);
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
                restoreSelectedNodes = CreateUpdateSelectionDelegate(sourceNodes);
                updateSelectedNode = CreateUpdateSelectionDelegate(target);
                updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            }

            GraphCommand reorder;
            var targetIndex = workflow.IndexOf(target);
            if (sinkNodes != null)
            {
                reorder = GetReversibleSort();
                addConnection += () => workflow.InsertRange(targetIndex, sinkNodes);
            }
            else reorder = GetSimpleSort();

            addConnection += reorder.Command;
            removeConnection += reorder.Undo;
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
            var sinkNodes = default(List<Node<ExpressionBuilder, ExpressionBuilderArgument>>);
            var totalRemoved = 0;
            foreach (var graphViewSource in graphViewSources)
            {
                var source = GetGraphNodeTag(workflow, graphViewSource);
                var predecessor = predecessorEdges.Where(xs => xs.Item1 == source).FirstOrDefault();
                if (predecessor == null) continue;
                if (predecessor.Item1.Successors.Count == 1)
                {
                    sinkNodes ??= new();
                    sinkNodes.Add(source);
                }

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

                totalRemoved++;
            }

            GraphCommand reorder;
            var components = workflow.FindConnectedComponents();
            var targetComponent = components.First(component => component.Contains(target));
            var targetIndex = LastIndexOfComponentNode(workflow, targetComponent) + 1;
            if (sinkNodes != null)
            {
                reorder = GetReversibleSort();
                removeConnection += () => workflow.InsertRange(targetIndex, sinkNodes);
            }
            else reorder = GetSimpleSort();

            removeConnection += reorder.Command;
            addConnection += reorder.Undo;
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
            Node<ExpressionBuilder, ExpressionBuilderArgument> target)
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
                commandExecutor.Execute(EmptyAction, SortWorkflow);
                commandExecutor.Execute(reorderConnection, restoreConnection);
                commandExecutor.Execute(SortWorkflow, EmptyAction);
            }
            else
            {
                var components = workflow.FindConnectedComponents();
                var sourceComponent = components.First(component => component.Contains(source));
                var targetComponent = components.First(component => component.Contains(target));
                if (sourceComponent == targetComponent) // reorder branches
                {
                    var reorder = GetReversibleSort();
                    var insertIndex = workflow.IndexOf(target);
                    commandExecutor.Execute(EmptyAction, reorder.Undo);
                    commandExecutor.Execute(() => workflow.Insert(insertIndex, source), EmptyAction);
                    commandExecutor.Execute(reorder.Command, EmptyAction);
                }
                else // reorder connected components
                {
                    var targetIndex = IndexOfComponentNode(workflow, targetComponent);
                    var reorder = GetReorderNodeRangeCommand(targetIndex, sourceComponent);
                    commandExecutor.Execute(reorder.Command, reorder.Undo);
                }
            }
        }

        GraphCommand GetReorderNodeRangeCommand(int index, IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> nodes)
        {
            var workflow = Workflow;
            var indexMap = RangeIndices(workflow, nodes);
            return new GraphCommand(
            () => workflow.InsertRange(index, indexMap.Keys),
            () =>
            {
                var insertOffset = indexMap.Count(item => item.Value > index);
                foreach (var restore in indexMap)
                {
                    var insertIndex = restore.Value > index
                        ? restore.Value + insertOffset--
                        : restore.Value;
                    workflow.Insert(insertIndex, restore.Key);
                }
            });
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
                if (!Workflow.Contains(node)) continue;
                try { ReorderGraphNode(node, targetNode); }
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
                if (group) return new MulticastSubject { Name = typeName };
                else return new SubscribeSubject { Name = typeName };
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

        static string MakeGenericType(string typeName, ExpressionBuilder builder, out ElementCategory elementCategory)
        {
            var separatorToken = typeName.IndexOf(',');
            var genericTypeName = typeName.Substring(0, separatorToken) + "`1" + typeName.Substring(separatorToken);
            var genericType = Type.GetType(genericTypeName);
            if (genericType == null)
            {
                throw new ArgumentException(Resources.TypeNotFound_Error, nameof(typeName));
            }

            var inspectBuilder = (InspectBuilder)builder;
            if (inspectBuilder?.ObservableType == null)
            {
                throw new ArgumentException(Resources.TypeNotFound_Error, nameof(builder));
            }

            var genericTypeAttributes = TypeDescriptor.GetAttributes(genericType);
            var elementCategoryAttribute = (WorkflowElementCategoryAttribute)genericTypeAttributes[typeof(WorkflowElementCategoryAttribute)];
            elementCategory = elementCategoryAttribute.Category;
            return genericType.MakeGenericType(inspectBuilder.ObservableType).AssemblyQualifiedName;
        }

        private void ConfigureBuilder(ExpressionBuilder builder, GraphNode selectedNode, string arguments)
        {
            if (string.IsNullOrEmpty(arguments)) return;
            // TODO: This special case for binary operator operands should be avoided in the future
            if (builder is BinaryOperatorBuilder binaryOperator && selectedNode != null)
            {
                if (GetGraphNodeTag(selectedNode).Value is InspectBuilder inputBuilder &&
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

        public void CreateGraphNode(
            string typeName,
            ElementCategory elementCategory,
            CreateGraphNodeType nodeType,
            bool branch,
            bool group)
        {
            CreateGraphNode(typeName, elementCategory, nodeType, branch, group, arguments: null);
        }

        public void CreateGraphNode(
            string typeName,
            ElementCategory elementCategory,
            CreateGraphNodeType nodeType,
            bool branch,
            bool group,
            string arguments)
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
                if (branch)
                {
                    if (selectedNodes.Length > 1)
                    {
                        GroupGraphNodes(
                            selectedNodes,
                            graph => new GroupWorkflowBuilder(graph),
                            node =>
                            {
                                var groupBuilder = node.Value;
                                var builder = CreateReplacementBuilder(
                                    groupBuilder,
                                    typeName,
                                    elementCategory,
                                    arguments,
                                    allowGroupReplacement: false);
                                ReplaceNode(node, builder);
                                return CreateUpdateSelectionDelegate(builder);
                            });
                    }
                    else ReplaceGraphNode(selectedNode, typeName, elementCategory, arguments);
                    return;
                }
                else if (elementCategory == ~ElementCategory.Combinator)
                {
                    typeName = MakeGenericType(typeName, selectedNode.Value, out elementCategory);
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
            var commands = GetCreateGraphNodeCommands(builder, selectedNodes.Select(GetGraphNodeTag), nodeType, branch);
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, commands.updateLayout.Undo);
            commandExecutor.Execute(commands.createNode.Command, commands.createNode.Undo);
            ReplaceExternalizedMappings(nodeType, selectedNodes);
            commandExecutor.Execute(commands.updateLayout.Command, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        public void CreateGraphNode(
            ExpressionBuilder builder,
            GraphNode selectedNode,
            CreateGraphNodeType nodeType,
            bool branch,
            bool validate = true,
            int insertIndex = -1)
        {
            CreateGraphNode(builder, GetGraphNodeTag(selectedNode), nodeType, branch, validate, insertIndex);
        }

        void CreateGraphNode(
            ExpressionBuilder builder,
            Node<ExpressionBuilder, ExpressionBuilderArgument> selectedNode,
            CreateGraphNodeType nodeType,
            bool branch,
            bool validate = true,
            int insertIndex = -1)
        {
            var selection = selectedNode != null ? new[] { selectedNode } : Enumerable.Empty<Node<ExpressionBuilder, ExpressionBuilderArgument>>();
            var commands = GetCreateGraphNodeCommands(builder, selection, nodeType, branch, validate, insertIndex);
            commandExecutor.Execute(
            () =>
            {
                commands.createNode.Command();
                commands.updateLayout.Command();
            },
            () =>
            {
                commands.createNode.Undo();
                commands.updateLayout.Undo();
            });
        }

        void ConfigureWorkflowBuilder(
            WorkflowExpressionBuilder workflowBuilder,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> selectedNodes,
            ExpressionBuilderGraph workflow,
            CreateGraphNodeType nodeType)
        {
            // Estimate number of inputs to the nested node
            var inputCount = nodeType == CreateGraphNodeType.Successor
                ? selectedNodes.Count(node => !node.Value.IsBuildDependency())
                : selectedNodes.Sum(node => workflow.PredecessorEdges(node).Count(edge => !edge.Item1.Value.IsBuildDependency()));
            inputCount = Math.Max(workflowBuilder.ArgumentRange.LowerBound, inputCount);

            // Limit number of inputs depending on nested operator argument range
            if (!(workflowBuilder is GroupWorkflowBuilder || workflowBuilder.GetType() == typeof(Defer)))
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

        (GraphCommand createNode, GraphCommand updateLayout) GetCreateGraphNodeCommands(
            ExpressionBuilder builder,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> selectedNodes,
            CreateGraphNodeType nodeType,
            bool branch,
            bool validate = true,
            int insertIndex = -1)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            var workflow = Workflow;
            if (builder is WorkflowInputBuilder workflowInput)
            {
                workflowInput.Index = workflow.Count(node => ExpressionBuilder.Unwrap(node.Value) is WorkflowInputBuilder);
            }

            var inspectBuilder = builder.AsInspectBuilder();
            var inspectNode = new Node<ExpressionBuilder, ExpressionBuilderArgument>(inspectBuilder);
            var inspectParameter = new ExpressionBuilderArgument();

            var targetNodes = selectedNodes.ToArray();
            if (targetNodes.Length > 0 &&
               (builder is ExternalizedMappingBuilder ||
                builder is AnnotationBuilder))
            {
                nodeType = CreateGraphNodeType.Predecessor;
            }

            var restoreSelectedNodes = CreateUpdateSelectionDelegate(targetNodes);
            if (insertIndex < 0)
            {
                insertIndex = GetInsertIndex(workflow, inspectBuilder, graphView.CursorNode, nodeType, branch);
            }

            builder = inspectBuilder.Builder;
            Action addNode = () =>
            {
                workflow.Insert(insertIndex, inspectNode);
                AddWorkflowInput(workflow, inspectNode);
            };
            Action removeNode = () =>
            {
                workflow.Remove(inspectNode);
                RemoveWorkflowInput(workflow, inspectNode);
            };

            if (builder is WorkflowExpressionBuilder workflowBuilder && validate)
            {
                ConfigureWorkflowBuilder(workflowBuilder, targetNodes, workflow, nodeType);
            }

            if (validate && !branch && targetNodes.Length > 1 &&
               ((nodeType == CreateGraphNodeType.Successor && targetNodes.Skip(1).Any(node => targetNodes[0].DepthFirstSearch().Contains(node))) ||
                (nodeType == CreateGraphNodeType.Predecessor && targetNodes.Skip(1).Any(node => node.DepthFirstSearch().Contains(targetNodes[0])))))
            {
                throw new InvalidOperationException(Resources.InsertValidation_Error);
            }

            var validateInsert = validate && !(
                nodeType == CreateGraphNodeType.Predecessor &&
                builder.IsBuildDependency() &&
                !targetNodes.Any(node => ExpressionBuilder.Unwrap(node.Value) switch
                {
                    AnnotationBuilder or ExternalizedMappingBuilder => true,
                    _ => false
                }));

            var reorder = nodeType == CreateGraphNodeType.Predecessor || targetNodes.Length == 0
                || targetNodes.Length == 1 && targetNodes[0] == graphView.CursorNode?.Tag
                ? GetSimpleSort() : GetReversibleSort();
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
                reorder.Command();
            };
            createNode.Undo = () =>
            {
                removeConnection();
                removeNode();
                reorder.Undo();
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

            return (createNode, updateLayout);
        }

        public void InsertGraphElements(ExpressionBuilderGraph elements, CreateGraphNodeType nodeType, bool branch)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            var sourceBuilder = elements.Sources().FirstOrDefault()?.Value;
            var insertIndex = GetInsertIndex(Workflow, sourceBuilder, graphView.CursorNode, nodeType, branch);
            InsertGraphElements(insertIndex, elements, nodeType, branch);
        }

        public void InsertGraphElements(int index, ExpressionBuilderGraph elements, CreateGraphNodeType nodeType, bool branch)
        {
            if (elements == null)
            {
                throw new ArgumentNullException(nameof(elements));
            }

            var selectedNodes = graphView.SelectedNodes.ToArray();
            var updateSelectedNodes = CreateUpdateSelectionDelegate(elements.Sinks().FirstOrDefault());
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();

            commandExecutor.Execute(EmptyAction, updateGraphLayout + restoreSelectedNodes);
            InsertGraphElements(index, elements, selectedNodes, nodeType, branch, EmptyAction, EmptyAction);
            commandExecutor.Execute(updateGraphLayout + updateSelectedNodes, EmptyAction);
        }

        private void InsertGraphElements(
            int index,
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
                Workflow.InsertRange(index, elements);
                foreach (var node in elements)
                {
                    AddWorkflowInput(Workflow, node);
                }
                addConnection();
            },
            () =>
            {
                removeConnection();
                Workflow.RemoveRange(index, elements.Count);
                foreach (var node in elements)
                {
                    RemoveWorkflowInput(Workflow, node);
                }
            });
            ReplaceExternalizedMappings(nodeType, selectedNodes);
        }

        void DeleteGraphNode(GraphNode node)
        {
            DeleteGraphNode(node, true);
        }

        void DeleteGraphNode(GraphNode node, bool replaceEdges, int index = -1)
        {
            var workflowNode = GetGraphNodeTag(Workflow, node);
            DeleteGraphNode(workflowNode, replaceEdges, index);
        }

        void DeleteGraphNode(
            Node<ExpressionBuilder, ExpressionBuilderArgument> workflowNode,
            bool replaceEdges,
            int index = -1)
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
                workflow.Remove(workflowNode);
                RemoveWorkflowInput(workflow, workflowNode);
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
                if (index < 0) workflow.Add(workflowNode);
                else workflow.Insert(index, workflowNode);
                AddWorkflowInput(workflow, workflowNode);
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
        }

        public void DeleteGraphNodes(IEnumerable<GraphNode> nodes)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (!nodes.Any()) return;
            var reorder = GetReversibleSort();
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, reorder.Undo + updateGraphLayout);
            foreach (var node in nodes)
            {
                DeleteGraphNode(node);
            }

            commandExecutor.Execute(reorder.Command + updateGraphLayout, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        public void MoveGraphNodes(IEnumerable<GraphNode> nodes, GraphNode target, CreateGraphNodeType nodeType, bool branch)
        {
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            updateGraphLayout += CreateUpdateSelectionDelegate(nodes);
            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, updateGraphLayout);
            var insertedNodes = nodes.SortSelection(Workflow).Select(GetGraphNodeTag).ToArray();
            var elements = insertedNodes.Convert(builder => builder);
            MoveNodesInternal(elements, insertedNodes, GetGraphNodeTag(target), new[] { target }, nodeType, branch);
            commandExecutor.Execute(updateGraphLayout, EmptyAction);
            commandExecutor.EndCompositeCommand();
        }

        private void MoveNodesInternal(
            ExpressionBuilderGraph elements,
            IEnumerable<Node<ExpressionBuilder, ExpressionBuilderArgument>> nodes,
            Node<ExpressionBuilder, ExpressionBuilderArgument> target,
            GraphNode[] selectedNodes,
            CreateGraphNodeType nodeType,
            bool branch)
        {
            if (elements.Count == 0) return;
            var buildDependencies = (from item in nodes.Zip(elements, (node, element) => new { node, element })
                                     from predecessor in Workflow.PredecessorEdges(item.node)
                                     where predecessor.Item1.Value.IsBuildDependency() && !elements.Any(node => node.Value == item.node.Value)
                                     orderby predecessor.Item3
                                     select new { predecessor, edge = Edge.Create(item.element, predecessor.Item2.Label) }).ToArray();
            commandExecutor.Execute(
                () => Array.ForEach(buildDependencies, dependency => Workflow.RemoveEdge(dependency.predecessor.Item1, dependency.predecessor.Item2)),
                () => Array.ForEach(buildDependencies, dependency => Workflow.InsertEdge(dependency.predecessor.Item1, dependency.predecessor.Item3, dependency.predecessor.Item2)));

            var reorder = GetReversibleSort();
            commandExecutor.Execute(EmptyAction, reorder.Undo);
            foreach (var node in nodes)
            {
                DeleteGraphNode(node, replaceEdges: true);
            }
            commandExecutor.Execute(reorder.Command, EmptyAction);

            var sourceBuilder = elements[0].Value;
            var insertIndex = GetInsertIndex(Workflow, sourceBuilder, target, nodeType, branch);
            Action addConnection = () => Array.ForEach(buildDependencies, dependency => Workflow.AddEdge(dependency.predecessor.Item1, dependency.edge));
            Action removeConnection = () => Array.ForEach(buildDependencies, dependency => Workflow.RemoveEdge(dependency.predecessor.Item1, dependency.edge));
            InsertGraphElements(insertIndex, elements, selectedNodes, nodeType, branch, addConnection, removeConnection);
        }

        private void ReplaceNode(GraphNode node, ExpressionBuilder builder)
        {
            ReplaceNode(GetGraphNodeTag(node), builder);
        }

        private void ReplaceNode(Node<ExpressionBuilder, ExpressionBuilderArgument> node, ExpressionBuilder builder)
        {
            var index = Workflow.IndexOf(node);
            CreateGraphNode(builder, node, CreateGraphNodeType.Successor, branch: false, validate: false, index);
            DeleteGraphNode(node, replaceEdges: true, index);
        }

        private bool CanGroup(IEnumerable<GraphNode> nodes, ExpressionBuilderGraph selectedElements)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            var workflow = this.Workflow;
            var selectedNodes = nodes.Select(node => (Node<ExpressionBuilder, ExpressionBuilderArgument>)node.Tag);
            return !(from node in selectedElements.Sources()
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

        private void GroupGraphNodes(
            IEnumerable<GraphNode> nodes,
            Func<ExpressionBuilderGraph, WorkflowExpressionBuilder> groupFactory,
            Func<Node<ExpressionBuilder, ExpressionBuilderArgument>, Action> continuation = null)
        {
            if (nodes == null)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (!nodes.Any()) return;
            var workflow = this.Workflow;
            GraphNode replacementNode = null;
            var nodeType = CreateGraphNodeType.Successor;
            var selectedElements = nodes.SortSelection(workflow).ToWorkflow(recurse: false);
            if (!CanGroup(nodes, selectedElements))
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
                                where !selectedElements.Any(n => n.Value == unwrapNode)
                                from successor in node.Successors
                                let unwrapSuccessor = ExpressionBuilder.Unwrap(successor.Target.Value)
                                let target = selectedElements.FirstOrDefault(n => n.Value == unwrapSuccessor)
                                where target != null
                                group new { successor.Label.Index, target } by node).ToArray();
            var successors = (from node in selectedElements
                              let workflowNode = workflow.Single(n => ExpressionBuilder.Unwrap(n.Value) == node.Value)
                              from successor in workflowNode.Successors
                              let unwrapSuccessor = ExpressionBuilder.Unwrap(successor.Target.Value)
                              where !selectedElements.Any(n => n.Value == unwrapSuccessor)
                              group new { successor, node, workflowNode } by successor.Target).ToArray();

            foreach (var predecessor in predecessors)
            {
                var workflowInput = new WorkflowInputBuilder { Index = inputIndex++ };
                var inputNode = selectedElements.Add(workflowInput);
                foreach (var edge in predecessor)
                {
                    selectedElements.AddEdge(inputNode, edge.target, new ExpressionBuilderArgument(edge.Index));
                }
            }

            var sinks = selectedElements.Sinks().ToArray();
            if (sinks.Length == 1 && !(sinks[0].Value is WorkflowOutputBuilder))
            {
                var sink = sinks.First();
                var workflowOutput = new WorkflowOutputBuilder();
                var outputNode = selectedElements.Add(workflowOutput);
                selectedElements.AddEdge(sink, outputNode, new ExpressionBuilderArgument());

                var sinkNode = graphView.Nodes.LayeredNodes().Single(node => GetGraphNodeBuilder(node) == sink.Value);
                if (sinkNode.Successors.Count() > 0)
                {
                    replacementNode = sinkNode;
                    nodeType = CreateGraphNodeType.Predecessor;
                }
            }

            SortWorkflow(selectedElements);
            var updateGraphLayout = CreateUpdateGraphLayoutDelegate();
            var workflowExpressionBuilder = groupFactory(selectedElements.ToInspectableGraph(recurse: false));
            var updateSelectedNode = CreateUpdateSelectionDelegate(workflowExpressionBuilder);
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(nodes.ToArray());

            var insertNode = default(Node<ExpressionBuilder, ExpressionBuilderArgument>);
            if (successors.Length > 0) insertNode = successors[0].Key;
            else
            {
                var components = workflow.FindConnectedComponents();
                var groupedSet = new HashSet<Node<ExpressionBuilder, ExpressionBuilderArgument>>(nodes.Select(GetGraphNodeTag));
                var targetComponent = components.Last(component => groupedSet.Overlaps(component));
                var lastNodeIndex = LastIndexOfComponentNode(workflow, targetComponent) + 1;
                insertNode = lastNodeIndex < workflow.Count ? workflow[lastNodeIndex] : null;
            }

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, () =>
            {
                updateGraphLayout();
                restoreSelectedNodes();
            });

            var reorder = GetReversibleSort();
            commandExecutor.Execute(EmptyAction, reorder.Undo);
            foreach (var node in nodes.Where(n => n != replacementNode))
            {
                DeleteGraphNode(node, replaceEdges: false);
            }

            var insertIndex = insertNode != null ? workflow.IndexOf(insertNode) : workflow.Count;
            CreateGraphNode(workflowExpressionBuilder,
                            replacementNode,
                            nodeType,
                            branch: false,
                            validate: false,
                            insertIndex);

            // Connect grouped node predecessors and successors
            var predecessorEdges = new List<(Node<ExpressionBuilder, ExpressionBuilderArgument> from, Edge<ExpressionBuilder, ExpressionBuilderArgument> edge)>();
            var successorEdges = new List<(Node<ExpressionBuilder, ExpressionBuilderArgument> from, Edge<ExpressionBuilder, ExpressionBuilderArgument> edge)>();
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
                        predecessorEdges.Add((predecessor.Key, edge));
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
                        successorEdges.Add((groupNode, edge));
                    }
                }
            },
            () =>
            {
                foreach (var (from, edge) in predecessorEdges)
                {
                    workflow.RemoveEdge(from, edge);
                }

                foreach (var (from, edge) in successorEdges)
                {
                    workflow.RemoveEdge(from, edge);
                }
            });

            if (replacementNode != null) DeleteGraphNode(replacementNode);
            if (continuation != null)
            {
                commandExecutor.Execute(updateGraphLayout, EmptyAction);
                try
                {
                    var groupNode = workflow.Single(node => ExpressionBuilder.Unwrap(node.Value) == workflowExpressionBuilder);
                    updateSelectedNode = continuation(groupNode) ?? updateSelectedNode;
                }
                catch (Exception)
                {
                    commandExecutor.EndCompositeCommand();
                    commandExecutor.Undo(allowRedo: false);
                    throw;
                }
            }
            commandExecutor.Execute(reorder.Command, EmptyAction);
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

        private ExpressionBuilder CreateReplacementBuilder(
            ExpressionBuilder selectedBuilder,
            string typeName,
            ElementCategory elementCategory,
            string arguments,
            bool allowGroupReplacement = true)
        {
            var workflow = Workflow;
            var inspectBuilder = (InspectBuilder)selectedBuilder;
            selectedBuilder = ExpressionBuilder.Unwrap(inspectBuilder);
            var selectedNode = FindWorkflowValue(workflow, selectedBuilder);
            var hasPredecessors = workflow.Predecessors(selectedNode).Any(node => !node.Value.IsBuildDependency());

            var allowGenericSource = elementCategory == ~ElementCategory.Combinator;
            if (allowGenericSource && (!hasPredecessors ||
               (selectedBuilder is SubjectExpressionBuilder && selectedBuilder.GetType().IsGenericType)))
            {
                typeName = MakeGenericType(typeName, inspectBuilder, out elementCategory);
            }

            ExpressionBuilder builder;
            if (allowGroupReplacement && selectedBuilder is WorkflowExpressionBuilder workflowBuilder &&
                typeof(WorkflowExpressionBuilder).IsAssignableFrom(Type.GetType(typeName)))
            {
                var replaceBuilder = CreateWorkflowBuilder(typeName, workflowBuilder.Workflow);
                replaceBuilder.Name = workflowBuilder.Name;
                replaceBuilder.Description = workflowBuilder.Description;
                builder = replaceBuilder;
            }
            else
            {
                var isReference = elementCategory == ~ElementCategory.Source;
                var preferMulticast = isReference && hasPredecessors;
                builder = CreateBuilder(typeName, elementCategory, group: preferMulticast);
                if (builder is WorkflowExpressionBuilder replacementWorkflowBuilder)
                {
                    ConfigureWorkflowBuilder(replacementWorkflowBuilder, new[] { selectedNode }, workflow, CreateGraphNodeType.Predecessor);
                }

                if (selectedBuilder is INamedElement namedBuilder &&
                   (namedBuilder is SubjectExpressionBuilder ||
                    namedBuilder is SubscribeSubject ||
                    namedBuilder is MulticastSubject))
                {
                    if (builder is SubjectExpressionBuilder subjectBuilder)
                    {
                        subjectBuilder.Name = namedBuilder.Name;
                    }
                    else if (builder is SubscribeSubject subscribeBuilder &&
                        string.IsNullOrEmpty(subscribeBuilder.Name))
                    {
                        subscribeBuilder.Name = namedBuilder.Name;
                    }
                    else if (builder is MulticastSubject multicastBuilder &&
                        string.IsNullOrEmpty(multicastBuilder.Name))
                    {
                        multicastBuilder.Name = namedBuilder.Name;
                    }
                }
            }
            ConfigureBuilder(builder, null, arguments);
            return builder;
        }

        public void ReplaceGraphNode(GraphNode node, string typeName, ElementCategory elementCategory, string arguments)
        {
            var selectedBuilder = GetGraphNodeBuilder(node);
            if (selectedBuilder.GetType().AssemblyQualifiedName == typeName)
            {
                return;
            }

            var builder = CreateReplacementBuilder(node.Value, typeName, elementCategory, arguments);
            ReplaceGraphNode(node, builder);
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
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);

            commandExecutor.BeginCompositeCommand();
            commandExecutor.Execute(EmptyAction, restoreSelectedNodes);
            commandExecutor.Execute(EmptyAction, updateGraphLayout);
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
                GroupWorkflowBuilder groupBuilder;
                var workflow = includeBuilder.Workflow;
                var path = includeBuilder.Path;
                if (workflow != null && !string.IsNullOrEmpty(path))
                {
                    UpgradeHelper.TryUpgradeWorkflow(workflow.FromInspectableGraph(), path, out workflow);
                    groupBuilder = new GroupWorkflowBuilder(workflow.ToInspectableGraph());
                }
                else groupBuilder = new GroupWorkflowBuilder();
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
            if (ExpressionBuilder.Unwrap(workflowNode.Value) is not WorkflowExpressionBuilder workflowBuilder)
            {
                // Do not ungroup disabled groups
                return;
            }

            var predecessors = workflow.PredecessorEdges(workflowNode).OrderBy(edge => edge.Item2.Label.Index).Select(xs => xs.Item1).ToArray();
            var successors = workflowNode.Successors.Select(xs => xs.Target).ToArray();
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

            var insertIndex = GetInsertIndex(Workflow, node, CreateGraphNodeType.Predecessor);
            DeleteGraphNode(node, replaceEdges: false, insertIndex);
            InsertGraphElements(
                insertIndex,
                groupWorkflow,
                Array.Empty<GraphNode>(),
                CreateGraphNodeType.Successor,
                false,
                EmptyAction,
                EmptyAction);

            // Connect incoming nodes to internal targets
            var mainSink = groupSinks.FirstOrDefault();
            var inputConnections = predecessors
                .Zip(groupSources, (xs, ys) =>
                    ys.Successors.SelectMany(zs => zs.Target != mainSink
                                     ? Enumerable.Repeat((source: xs, target: zs.Target), 1)
                                     : successors.Select(ss => (source: xs, target: ss))));
            foreach (var (source, target) in inputConnections.SelectMany(xs => xs))
            {
                ConnectInternalNodes(source, target);
            }

            // Connect output sources to external targets
            var outputConnections = groupOutputs
                .Select(edge => edge.Item1)
                .Where(xs => xs != null && groupWorkflow.Contains(xs))
                .SelectMany(xs => successors.Select(successor =>
                    (source: xs, target: successor)));
            foreach (var (source, target) in outputConnections)
            {
                ConnectInternalNodes(source, target);
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
            var subscribeDependents = new List<SubscribeSubject>();
            var multicastDependents = new List<MulticastSubject>();
            foreach (var dependent in definition.GetDependentExpressions()
                                                .SelectMany(context => context)
                                                .Where(element => element.Builder is INamedElement namedElement &&
                                                    namedElement.Name == definition.Subject.Name))
            {
                if (dependent.IsReadOnly) continue;
                else if (dependent.Builder is SubscribeSubject subscribeSubject)
                {
                    subscribeDependents.Add(subscribeSubject);
                }
                else if (dependent.Builder is MulticastSubject multicastSubject)
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

        public void NavigateTo(WorkflowEditorPath path)
        {
            if (path == workflowPath)
                return;

            var previousPath = workflowPath;
            var selectedNodes = graphView.SelectedNodes.ToArray();
            var restoreSelectedNodes = CreateUpdateSelectionDelegate(selectedNodes);
            commandExecutor.Execute(
                () => WorkflowPath = path,
                () =>
                {
                    WorkflowPath = previousPath;
                    restoreSelectedNodes();
                });
        }

        public void ResetNavigation()
        {
            WorkflowPath = null;
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
