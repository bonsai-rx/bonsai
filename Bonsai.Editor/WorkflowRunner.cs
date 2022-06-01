using Bonsai.Design;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace Bonsai.Editor
{
    public class WorkflowRunner
    {
        static void RunLayout(
            string fileName,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            WorkflowBuilder workflowBuilder,
            VisualizerLayout layout)
        {
            var typeVisualizers = new TypeVisualizerMap();
            var loadVisualizers = (from typeVisualizer in visualizerProvider
                                   let targetType = Type.GetType(typeVisualizer.TargetTypeName)
                                   let visualizerType = Type.GetType(typeVisualizer.VisualizerTypeName)
                                   where targetType != null && visualizerType != null
                                   select (targetType, visualizerType))
                                   .Do(entry => typeVisualizers.Add(entry.targetType, entry.visualizerType))
                                   .ToEnumerable().ToList();

            workflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.ToInspectableGraph());
            LayoutHelper.SetWorkflowNotifications(workflowBuilder.Workflow, publishNotifications: false);
            foreach (var node in workflowBuilder.Workflow)
            {
                var layoutSettings = layout.GetLayoutSettings(node.Value);
                if (layoutSettings == null)
                {
                    layoutSettings = new VisualizerDialogSettings();
                    layout.DialogSettings.Add(layoutSettings);
                }
                layoutSettings.Tag = node.Value;
            }

            LayoutHelper.SetLayoutNotifications(layout);
            var services = new System.ComponentModel.Design.ServiceContainer();
            services.AddService(typeof(WorkflowBuilder), workflowBuilder);
            var runtimeWorkflow = workflowBuilder.Workflow.BuildObservable();
            var mapping = LayoutHelper.CreateVisualizerMapping(workflowBuilder.Workflow, layout, typeVisualizers, services);

            var cts = new CancellationTokenSource();
            var contextMenu = new ContextMenuStrip();
            foreach (var launcher in mapping.Values.Where(launcher => launcher.Visualizer.IsValueCreated))
            {
                var activeLauncher = launcher;
                contextMenu.Items.Add(new ToolStripMenuItem(launcher.Text, null, (sender, e) =>
                {
                    activeLauncher.Show(services);
                }));
            }
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Stop", null, (sender, e) => cts.Cancel()));

            var notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.Text = Path.GetFileName(fileName);
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;
            runtimeWorkflow.Finally(() =>
            {
                notifyIcon.Visible = false;
                Application.Exit();
            }).Subscribe(
                unit => { },
                ex => { Console.WriteLine(ex); },
                () => { },
                cts.Token);
            Application.Run();
        }

        static void RunHeadless(WorkflowBuilder workflowBuilder)
        {
            var workflowCompleted = new ManualResetEvent(false);
            workflowBuilder.Workflow.BuildObservable().Subscribe(
                unit => { },
                ex => { Console.WriteLine(ex); workflowCompleted.Set(); },
                () => workflowCompleted.Set());
            workflowCompleted.WaitOne();
        }

        public static void Run(string fileName, Dictionary<string, string> propertyAssignments, IObservable<TypeVisualizerDescriptor> visualizerProvider = null)
        {
            Run(fileName, propertyAssignments, visualizerProvider);
        }

        public static void Run(
            string fileName,
            Dictionary<string, string> propertyAssignments,
            IObservable<TypeVisualizerDescriptor> visualizerProvider = null,
            string layoutPath = null)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!File.Exists(fileName))
            {
                throw new ArgumentException("Specified workflow file does not exist.", nameof(fileName));
            }

            WorkflowBuilder workflowBuilder;
            using (var reader = XmlReader.Create(fileName))
            {
                reader.MoveToContent();
                var serializer = new XmlSerializer(typeof(WorkflowBuilder), reader.NamespaceURI);
                workflowBuilder = (WorkflowBuilder)serializer.Deserialize(reader);
            }

            workflowBuilder.Workflow.Build();
            foreach (var assignment in propertyAssignments)
            {
                workflowBuilder.Workflow.SetWorkflowProperty(assignment.Key, assignment.Value);
            }

            layoutPath ??= LayoutHelper.GetLayoutPath(fileName);
            if (visualizerProvider != null && File.Exists(layoutPath))
            {
                VisualizerLayout layout = null;
                using (var reader = XmlReader.Create(layoutPath))
                {
                    layout = (VisualizerLayout)VisualizerLayout.Serializer.Deserialize(reader);
                }

                RunLayout(fileName, visualizerProvider, workflowBuilder, layout);
            }
            else RunHeadless(workflowBuilder);
        }
    }
}
