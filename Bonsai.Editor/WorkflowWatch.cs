using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Bonsai.Editor.Diagnostics;
using Bonsai.Expressions;

namespace Bonsai.Editor
{
    internal class WorkflowWatch
    {
        const int WatchPeriod = 100;
        readonly Timer watchTimer = new() { Interval = WatchPeriod };
        WorkflowMeter workflowMeter;

        public WorkflowWatch()
        {
            watchTimer.Tick += (_, e) => OnUpdate(e);
        }

        public event EventHandler Update;

        private void OnUpdate(EventArgs e)
        {
            Update?.Invoke(this, e);
        }

        public void Start(ExpressionBuilderGraph workflow)
        {
            if (workflowMeter is not null)
            {
                throw new InvalidOperationException(
                    $"{nameof(Stop)} must be called before starting a new watch.");
            }

            workflowMeter = new WorkflowMeter(workflow);
            OnUpdate(EventArgs.Empty);
            watchTimer.Start();
        }

        public IReadOnlyDictionary<ExpressionBuilder, WorkflowElementCounter> Counters =>
            workflowMeter?.Counters;

        public void Stop()
        {
            watchTimer.Stop();
            if (workflowMeter is not null)
            {
                workflowMeter.Dispose();
                workflowMeter = null;
            }
            OnUpdate(EventArgs.Empty);
        }
    }
}
