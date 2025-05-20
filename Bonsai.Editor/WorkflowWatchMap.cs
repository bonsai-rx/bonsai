using System;
using System.Collections.Generic;
using System.Linq;
using Bonsai.Editor.Diagnostics;
using Bonsai.Editor.GraphModel;
using Bonsai.Editor.GraphView;
using Bonsai.Expressions;

namespace Bonsai.Editor
{
    internal class WorkflowWatchMap
    {
        readonly HashSet<InspectBuilder> watchSet = new();

        public int Count => watchSet.Count;

        public bool Add(InspectBuilder item)
        {
            return watchSet.Add(item);
        }

        public bool Contains(ExpressionBuilder builder)
        {
            return builder is InspectBuilder inspectBuilder && Contains(inspectBuilder);
        }

        public bool Contains(InspectBuilder item)
        {
            return watchSet.Contains(item);
        }

        public bool Remove(InspectBuilder item)
        {
            return watchSet.Remove(item);
        }

        internal void Clear()
        {
            watchSet.Clear();
        }

        internal void InitializeWatchState(WorkflowEditor editor)
        {
            foreach (var node in editor.GraphView.Nodes.LayeredNodes())
            {
                node.Status = watchSet.Contains(node.Value)
                    ? WorkflowElementStatus.Ready
                    : null;
            }
        }

        internal void SetWatchNotifications(WorkflowBuilder source)
        {
            foreach (var watch in watchSet)
            {
                watch.PublishNotifications = true;
            }
        }

        public WorkflowWatchSettings GetWatchSettings(WorkflowBuilder workflowBuilder)
        {
            var settings = new WorkflowWatchSettings();
            foreach (var watch in workflowBuilder.FindAll(Contains, unwrap: false))
                settings.WatchList.Add(new WorkflowElementWatchSettings { Path = watch.Path });
            return settings;
        }

        public void SetWatchSettings(WorkflowBuilder workflowBuilder, WorkflowWatchSettings settings)
        {
            watchSet.Clear();
            foreach (var watch in settings.WatchList)
            {
                try
                {
                    var inspectBuilder = (InspectBuilder)watch.Path.Resolve(workflowBuilder);
                    watchSet.Add(inspectBuilder);
                }
                catch { continue; } // best effort, drop any unresolved watch
            }
        }
    }
}
