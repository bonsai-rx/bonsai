﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using Bonsai.Dag;

namespace Bonsai.Design
{
    class WorkflowEditorLauncher : DialogLauncher
    {
        bool userClosing;
        WorkflowExpressionBuilder builder;
        WorkflowGraphView workflowGraphView;

        public WorkflowEditorLauncher(WorkflowExpressionBuilder builder, WorkflowGraphView parentView)
        {
            if (builder == null)
            {
                throw new ArgumentNullException("builder");
            }

            if (parentView == null)
            {
                throw new ArgumentNullException("parentView");
            }

            this.builder = builder;
            ParentView = parentView;
        }

        internal WorkflowGraphView ParentView { get; private set; }

        internal IWin32Window Owner
        {
            get { return VisualizerDialog; }
        }

        public VisualizerLayout VisualizerLayout { get; set; }

        public WorkflowGraphView WorkflowGraphView
        {
            get { return workflowGraphView; }
        }

        public void UpdateEditorLayout()
        {
            if (workflowGraphView != null)
            {
                workflowGraphView.UpdateVisualizerLayout();
                VisualizerLayout = workflowGraphView.VisualizerLayout;
                if (VisualizerDialog != null)
                {
                    Bounds = VisualizerDialog.DesktopBounds;
                }
            }
        }

        public override void Hide()
        {
            userClosing = false;
            base.Hide();
        }

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            userClosing = true;
            visualizerDialog.Activated += delegate
            {
                if (!string.IsNullOrWhiteSpace(builder.Name))
                {
                    visualizerDialog.Text = builder.Name;
                }
                else visualizerDialog.Text = "Workflow Editor";
            };

            visualizerDialog.FormClosing += (sender, e) =>
            {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    if (userClosing)
                    {
                        e.Cancel = true;
                        workflowGraphView.CloseWorkflowEditorLauncher(this);
                    }
                    else UpdateEditorLayout();
                }
            };

            workflowGraphView = new WorkflowGraphView(provider);
            workflowGraphView.Launcher = this;
            workflowGraphView.Dock = DockStyle.Fill;
            workflowGraphView.Size = new Size(300, 200);
            workflowGraphView.Workflow = builder.Workflow;
            workflowGraphView.VisualizerLayout = VisualizerLayout;
            visualizerDialog.Padding = new Padding(10);
            visualizerDialog.AddControl(workflowGraphView);
        }
    }
}
