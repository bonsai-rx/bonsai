using System;
using System.Linq;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Windows.Forms;
using Bonsai.Editor;
using Bonsai.Editor.GraphView;

namespace Bonsai.Design
{
    class VisualizerDialogLauncher : DialogLauncher, ITypeVisualizerContext
    {
        IDisposable visualizerObserver;
        InspectBuilder visualizerSource;
        Lazy<DialogTypeVisualizer> visualizer;
        WorkflowGraphView workflowGraphView;
        ServiceContainer visualizerContext;
        InspectBuilder source;

        public VisualizerDialogLauncher(
            InspectBuilder visualizerSource,
            Func<DialogTypeVisualizer> visualizerFactory,
            WorkflowGraphView workflowGraphView,
            InspectBuilder workflowSource)
        {
            if (visualizerSource == null)
            {
                throw new ArgumentNullException("visualizerSource");
            }

            if (visualizerFactory == null)
            {
                throw new ArgumentNullException("visualizerFactory");
            }

            if (workflowGraphView == null)
            {
                throw new ArgumentNullException("workflowGraphView");
            }

            this.visualizerSource = visualizerSource;
            this.visualizer = new Lazy<DialogTypeVisualizer>(visualizerFactory);
            this.workflowGraphView = workflowGraphView;
            this.source = workflowSource ?? visualizerSource;
        }

        public string Text { get; set; }

        public InspectBuilder Source
        {
            get { return source; }
        }

        public Lazy<DialogTypeVisualizer> Visualizer
        {
            get { return visualizer; }
        }

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            visualizerDialog.Text = Text;
            visualizerContext = new ServiceContainer(provider);
            visualizerContext.AddService(typeof(ITypeVisualizerContext), this);
            visualizerContext.AddService(typeof(TypeVisualizerDialog), visualizerDialog);
            visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
            visualizerContext.AddService(typeof(ExpressionBuilderGraph), workflowGraphView.Workflow);
            visualizer.Value.Load(visualizerContext);
            var visualizerOutput = visualizer.Value.Visualize(visualizerSource.Output, visualizerContext);

            visualizerDialog.AllowDrop = true;
            visualizerDialog.KeyPreview = true;
            visualizerDialog.KeyDown += new KeyEventHandler(visualizerDialog_KeyDown);
            visualizerDialog.DragEnter += new DragEventHandler(visualizerDialog_DragEnter);
            visualizerDialog.DragOver += new DragEventHandler(visualizerDialog_DragOver);
            visualizerDialog.DragDrop += new DragEventHandler(visualizerDialog_DragDrop);
            visualizerDialog.Load += delegate
            {
                visualizerObserver = visualizerOutput.Subscribe(
                    xs => { },
                    ex => visualizerDialog.BeginInvoke((Action)(() =>
                    {
                        MessageBox.Show(visualizerDialog, ex.Message, visualizerDialog.Text);
                        visualizerDialog.Close();
                    })));
            };

            visualizerDialog.HandleDestroyed += delegate
            {
                if (visualizerObserver != null)
                {
                    visualizerObserver.Dispose();
                    visualizerObserver = null;
                }
            };

            visualizerDialog.FormClosed += delegate
            {
                visualizer.Value.Unload();
                visualizerContext.RemoveService(typeof(ExpressionBuilderGraph));
                visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));
                visualizerContext.RemoveService(typeof(TypeVisualizerDialog));
                visualizerContext.RemoveService(typeof(ITypeVisualizerContext));
                visualizerContext.Dispose();
                visualizerContext = null;
            };

            visualizerDialog.Shown += delegate
            {
                visualizerDialog.Activated += delegate
                {
                    workflowGraphView.SelectBuilderNode(source);
                };
            };
        }

        void visualizerDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && e.Control)
            {
                workflowGraphView.SelectBuilderNode(source);
                workflowGraphView.EditorControl.ParentForm.Activate();
            }

            if (e.KeyCode == Keys.Delete && e.Control)
            {
                var dialogMashup = visualizer.Value as DialogMashupVisualizer;
                if (dialogMashup != null && dialogMashup.Mashups.Count > 0)
                {
                    UnloadMashups();
                    dialogMashup.Mashups.RemoveAt(dialogMashup.Mashups.Count - 1);
                    ReloadMashups();
                }
            }
        }

        void ReloadMashups()
        {
            var dialogMashup = visualizer.Value as DialogMashupVisualizer;
            if (visualizerObserver != null && dialogMashup != null)
            {
                dialogMashup.LoadMashups(visualizerContext);
                var visualizerOutput = visualizer.Value.Visualize(visualizerSource.Output, visualizerContext);
                visualizerObserver = visualizerOutput.Subscribe();
            }
        }

        void UnloadMashups()
        {
            var dialogMashup = visualizer.Value as DialogMashupVisualizer;
            if (visualizerObserver != null && dialogMashup != null)
            {
                visualizerObserver.Dispose();
                dialogMashup.UnloadMashups();
            }
        }

        static Type GetMashupVisualizerType(Type dialogMashupType, Type visualizerType, IWorkflowEditorService editorService)
        {
            var mashupVisualizerType = default(Type);
            while (dialogMashupType != null && dialogMashupType != typeof(DialogMashupVisualizer))
            {
                var mashup = typeof(VisualizerMashup<,>).MakeGenericType(dialogMashupType, visualizerType);
                mashupVisualizerType = editorService.GetTypeVisualizers(mashup).SingleOrDefault();
                if (mashupVisualizerType != null) break;
                dialogMashupType = dialogMashupType.BaseType;
            }

            return mashupVisualizerType;
        }

        public void CreateMashup(GraphNode graphNode, IWorkflowEditorService editorService)
        {
            var dialogMashup = visualizer.Value as DialogMashupVisualizer;
            var visualizerDialog = workflowGraphView.GetVisualizerDialogLauncher(graphNode);
            if (dialogMashup != null && visualizerDialog != null)
            {
                var dialogMashupType = dialogMashup.GetType();
                var visualizerType = visualizerDialog.visualizer.Value.GetType();
                var mashupVisualizer = GetMashupVisualizerType(dialogMashupType, visualizerType, editorService);
                if (mashupVisualizer != null)
                {
                    UnloadMashups();
                    var visualizerMashup = (MashupTypeVisualizer)Activator.CreateInstance(mashupVisualizer);
                    dialogMashup.Mashups.Add(new VisualizerMashup(visualizerDialog.visualizerSource.Output, visualizerMashup));
                    ReloadMashups();
                }
            }
        }

        void visualizerDialog_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GraphNode)) && visualizerContext != null)
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var editorService = (IWorkflowEditorService)visualizerContext.GetService(typeof(IWorkflowEditorService));
                CreateMashup(graphNode, editorService);
            }
        }

        void visualizerDialog_DragOver(object sender, DragEventArgs e)
        {
        }

        void visualizerDialog_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GraphNode)) && visualizerContext != null)
            {
                var graphViewSource = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var visualizerDialog = workflowGraphView.GetVisualizerDialogLauncher(graphViewSource);
                if (visualizerDialog != null)
                {
                    var dialogMashupType = visualizer.Value.GetType();
                    if (dialogMashupType.IsSubclassOf(typeof(DialogMashupVisualizer)))
                    {
                        var visualizerType = visualizerDialog.Visualizer.Value.GetType();
                        var editorService = (IWorkflowEditorService)visualizerContext.GetService(typeof(IWorkflowEditorService));
                        var mashupVisualizerType = GetMashupVisualizerType(dialogMashupType, visualizerType, editorService);
                        if (mashupVisualizerType != null)
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
            }
        }
    }
}
