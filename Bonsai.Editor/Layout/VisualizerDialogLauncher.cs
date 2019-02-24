using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Drawing;
using System.Windows.Forms;
using Bonsai.Dag;
using Bonsai.Editor;

namespace Bonsai.Design
{
    class VisualizerDialogLauncher : DialogLauncher, ITypeVisualizerContext
    {
        InspectBuilder source;
        IDisposable visualizerObserver;
        Lazy<DialogTypeVisualizer> visualizer;
        WorkflowGraphView workflowGraphView;
        ServiceContainer visualizerContext;

        public VisualizerDialogLauncher(InspectBuilder source, Func<DialogTypeVisualizer> visualizerFactory, WorkflowGraphView workflowGraphView)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (visualizerFactory == null)
            {
                throw new ArgumentNullException("visualizerFactory");
            }

            if (workflowGraphView == null)
            {
                throw new ArgumentNullException("workflowGraphView");
            }

            this.source = source;
            this.visualizer = new Lazy<DialogTypeVisualizer>(visualizerFactory);
            this.workflowGraphView = workflowGraphView;
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
            var visualizerOutput = visualizer.Value.Visualize(source.Output, visualizerContext);

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

            visualizerDialog.Activated += delegate
            {
                workflowGraphView.SelectBuilderNode(source);
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
                var visualizerOutput = visualizer.Value.Visualize(source.Output, visualizerContext);
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

        public void CreateMashup(GraphNode graphNode, IWorkflowEditorService editorService)
        {
            var dialogMashup = visualizer.Value as DialogMashupVisualizer;
            var visualizerDialog = workflowGraphView.GetVisualizerDialogLauncher(graphNode);
            if (dialogMashup != null && visualizerDialog != null)
            {
                var dialogMashupType = dialogMashup.GetType();
                var visualizerType = visualizerDialog.visualizer.Value.GetType();
                var mashupVisualizer = default(Type);
                while (dialogMashupType != null)
                {
                    var mashup = typeof(VisualizerMashup<,>).MakeGenericType(dialogMashupType, visualizerType);
                    mashupVisualizer = editorService.GetTypeVisualizers(mashup).SingleOrDefault();
                    if (mashupVisualizer != null) break;
                    dialogMashupType = dialogMashupType.BaseType;
                }

                if (mashupVisualizer != null)
                {
                    UnloadMashups();
                    var visualizerMashup = (MashupTypeVisualizer)Activator.CreateInstance(mashupVisualizer);
                    dialogMashup.Mashups.Add(new VisualizerMashup(visualizerDialog.Source.Output, visualizerMashup));
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
                    var visualizerType = visualizer.Value.GetType();
                    if (visualizerType.IsSubclassOf(typeof(DialogMashupVisualizer)))
                    {
                        var editorService = (IWorkflowEditorService)visualizerContext.GetService(typeof(IWorkflowEditorService));
                        var mashup = typeof(VisualizerMashup<,>).MakeGenericType(visualizer.Value.GetType(), visualizerDialog.Visualizer.Value.GetType());
                        var mashupVisualizer = editorService.GetTypeVisualizers(mashup);
                        if (mashupVisualizer != null)
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
            }
        }
    }
}
