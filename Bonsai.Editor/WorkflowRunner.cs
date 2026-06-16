using Bonsai.Design;
using Bonsai.Editor.GraphModel;
using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;

namespace Bonsai.Editor
{
    public class WorkflowRunner
    {
        static void RunLayout(
            WorkflowBuilder workflowBuilder,
            WorkflowRuntimeExceptionCache exceptionCache,
            Dictionary<string, string> propertyAssignments,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            VisualizerLayout layout,
            string fileName,
            bool debugger)
        {
            var typeVisualizers = new TypeVisualizerMap();
            var loadVisualizers = (from typeVisualizer in visualizerProvider
                                   let targetType = Type.GetType(typeVisualizer.TargetTypeName)
                                   let visualizerType = Type.GetType(typeVisualizer.VisualizerTypeName)
                                   where targetType != null && visualizerType != null
                                   select (targetType, visualizerType))
                                   .Do(entry => typeVisualizers.Add(entry.targetType, entry.visualizerType))
                                   .ToEnumerable().ToList();
            BuildAssignProperties(workflowBuilder, propertyAssignments);

            var visualizerSettings = VisualizerLayoutMap.FromVisualizerLayout(workflowBuilder, layout, typeVisualizers);
            var visualizerWindows = visualizerSettings.CreateVisualizerWindows(workflowBuilder);
            LayoutHelper.SetWorkflowNotifications(workflowBuilder.Workflow, publishNotifications: debugger);
            LayoutHelper.SetLayoutNotifications(workflowBuilder.Workflow, visualizerWindows);

            var services = new System.ComponentModel.Design.ServiceContainer();
            services.AddService(typeof(WorkflowBuilder), workflowBuilder);
            var runtimeWorkflow = workflowBuilder.Workflow.BuildObservable();
            using var exceptionSubscription = SubscribeRuntimeExceptionCache(workflowBuilder, exceptionCache);

            var cts = new CancellationTokenSource();
            var contextMenu = new ContextMenuStrip();
            foreach (var launcher in visualizerWindows)
            {
                var activeLauncher = launcher;
                contextMenu.Items.Add(new ToolStripMenuItem(launcher.Text, null, (sender, e) =>
                {
                    activeLauncher.Show(services);
                }));
            }
            contextMenu.Items.Add(new ToolStripSeparator());
            contextMenu.Items.Add(new ToolStripMenuItem("Stop", null, (sender, e) => cts.Cancel()));

            using var notifyIcon = new NotifyIcon();
            notifyIcon.Icon = Properties.Resources.Icon;
            notifyIcon.Text = Path.GetFileName(fileName);
            notifyIcon.ContextMenuStrip = contextMenu;
            notifyIcon.Visible = true;

            visualizerWindows.Show(visualizerSettings, services);
            using var synchronizationContext = new WindowsFormsSynchronizationContext();
            runtimeWorkflow.Finally(() =>
            {
                // Posting the exit to the main thread's winforms sync context is important for two reasons:
                // 1) When this finally action executes on the main thread we need to defer exiting until
                //    Application.Run, otherwise we're trying to exit a message loop which hasn't even started.
                // 2) When this finally action executes it will be on a background thread, we need to exit from the main
                //    thread. While Application.Exit can be called from any thread, it directly calls FormClosed callbacks
                //    of any open forms, and many visualizers assume they'll only be called from the main thread.
                synchronizationContext.Post(_ => Application.Exit(), null);
            }).Subscribe(
                unit => { },
                ex => LogWorkflowExceptionStackTrace(fileName, workflowBuilder, exceptionCache, ex),
                () => { },
                cts.Token);

            Application.Run();
        }

        static void RunHeadless(
            WorkflowBuilder workflowBuilder,
            WorkflowRuntimeExceptionCache exceptionCache,
            Dictionary<string, string> propertyAssignments,
            string fileName)
        {
            BuildAssignProperties(workflowBuilder, propertyAssignments);
            var runtimeWorkflow = workflowBuilder.Workflow.BuildObservable();
            using var exceptionSubscription = SubscribeRuntimeExceptionCache(workflowBuilder, exceptionCache);

            var workflowCompleted = new ManualResetEvent(false);
            runtimeWorkflow.Subscribe(
                unit => { },
                ex =>
                {
                    LogWorkflowExceptionStackTrace(fileName, workflowBuilder, exceptionCache, ex);
                    workflowCompleted.Set();
                },
                () => workflowCompleted.Set());
            workflowCompleted.WaitOne();
        }

        static IDisposable SubscribeRuntimeExceptionCache(WorkflowBuilder workflowBuilder, WorkflowRuntimeExceptionCache exceptionCache)
        {
            return workflowBuilder.Workflow.InspectErrorsEx().Synchronize().Subscribe(ex =>
            {
                if (ex is WorkflowRuntimeException workflowException)
                {
                    exceptionCache.TryAdd(workflowException);
                }
            });
        }

        static void BuildAssignProperties(WorkflowBuilder workflowBuilder, Dictionary<string, string> propertyAssignments)
        {
            workflowBuilder.Workflow.Build();
            foreach (var assignment in propertyAssignments)
            {
                workflowBuilder.Workflow.SetWorkflowProperty(assignment.Key, assignment.Value);
            }
        }

        public static void Run(string fileName, Dictionary<string, string> propertyAssignments, IObservable<TypeVisualizerDescriptor> visualizerProvider = null)
        {
            Run(fileName, propertyAssignments, visualizerProvider, layoutPath: null);
        }

        public static void Run(
            string fileName,
            Dictionary<string, string> propertyAssignments,
            IObservable<TypeVisualizerDescriptor> visualizerProvider = null,
            string layoutPath = null)
        {
            Run(fileName, propertyAssignments, visualizerProvider, layoutPath, debugger: true);
        }

        public static void Run(
            string fileName,
            Dictionary<string, string> propertyAssignments,
            IObservable<TypeVisualizerDescriptor> visualizerProvider,
            string layoutPath,
            bool debugger)
        {
            if (string.IsNullOrEmpty(fileName))
            {
                throw new ArgumentNullException(nameof(fileName));
            }

            if (!File.Exists(fileName))
            {
                throw new ArgumentException("Specified workflow file does not exist.", nameof(fileName));
            }

            var workflowBuilder = ElementStore.LoadWorkflow(fileName);
            var settingsPath = Project.GetWorkflowSettingsDirectory(fileName);
            layoutPath ??= LayoutHelper.GetCompatibleLayoutPath(settingsPath, fileName);

            var hasLayout = File.Exists(layoutPath);
            var hasVisualizerWindow = workflowBuilder.Workflow.Descendants().OfType<VisualizerWindow>().Any();
            var runLayout = visualizerProvider != null && (hasLayout || hasVisualizerWindow);
            if (debugger || runLayout)
                workflowBuilder = new WorkflowBuilder(workflowBuilder.Workflow.ToInspectableGraph());

            var exceptionCache = new WorkflowRuntimeExceptionCache();
            try
            {
                if (runLayout)
                {
                    var layout = hasLayout ? VisualizerLayout.Load(layoutPath) : new VisualizerLayout();
                    RunLayout(workflowBuilder, exceptionCache, propertyAssignments, visualizerProvider, layout, fileName, debugger);
                }
                else RunHeadless(workflowBuilder, exceptionCache, propertyAssignments, fileName);
            }
            catch (WorkflowException ex)
            {
                LogWorkflowExceptionStackTrace(fileName, workflowBuilder, exceptionCache, ex);
            }
        }

        static void LogWorkflowExceptionStackTrace(
            string fileName,
            WorkflowBuilder workflowBuilder,
            WorkflowRuntimeExceptionCache exceptionCache,
            Exception ex)
        {
            if (exceptionCache.TryGetWorkflowException(ex, out var workflowException))
            {
                var sb = new StringBuilder();
                sb.AppendLine(ex.Message);
                WriteWorkflowExceptionStackTrace(fileName, workflowException, workflowBuilder, sb);
                Console.Error.WriteLine(sb);
                Console.Error.WriteLine("   --- Runtime exception stack trace ---");
            }
            Console.Error.WriteLine(ex);
        }

        static void WriteWorkflowExceptionStackTrace(
            string path,
            WorkflowException ex,
            WorkflowBuilder workflowBuilder,
            StringBuilder stringBuilder)
        {
            using var stream = UpgradeHelper.GetWorkflowStream(path);
            using var reader = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = true });
            WriteWorkflowExceptionStackTrace(path, reader, ex, workflowBuilder, stringBuilder);
        }

        static void WriteWorkflowExceptionStackTrace(
            string path,
            XmlReader reader,
            WorkflowException ex,
            WorkflowBuilder workflowBuilder,
            StringBuilder stringBuilder)
        {
            if (reader is IXmlLineInfo lineInfo)
            {
                var workflowPath = WorkflowEditorPath.GetBuilderPath(workflowBuilder, ex.Builder);
                if (reader.ReadToDescendant(nameof(ExpressionBuilderGraphDescriptor.Nodes)))
                {
                    reader.ReadStartElement();
                    for (var i = 0; i <= workflowPath.Index; i++)
                    {
                        if (!reader.ReadToNextSibling("Expression"))
                            return;
                    }

                    var builder = ExpressionBuilder.Unwrap(ex.Builder);
                    var element = ExpressionBuilder.GetWorkflowElement(builder);
                    var typeName = element.GetType().FullName;
                    var elementName = (element as INamedElement)?.Name;
                    var lineNumber = lineInfo.LineNumber;

                    if (ex.InnerException is WorkflowException innerException)
                    {
                        if (builder is IncludeWorkflowBuilder includeBuilder)
                            WriteWorkflowExceptionStackTrace(includeBuilder.Path, innerException, workflowBuilder, stringBuilder);
                        else
                            WriteWorkflowExceptionStackTrace(path, reader, innerException, workflowBuilder, stringBuilder);
                    }

                    var location = $"in {path}:line {lineNumber}";
                    if (string.IsNullOrEmpty(elementName))
                        stringBuilder.AppendLine($"   at {typeName} {location}");
                    else
                        stringBuilder.AppendLine($"   at {typeName} named {elementName} {location}");
                }
            }
        }
    }
}
