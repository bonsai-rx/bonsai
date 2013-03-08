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

namespace Bonsai.Design
{
    public class VisualizerDialogLauncher : DialogLauncher, ITypeVisualizerContext
    {
        InspectBuilder source;
        DialogTypeVisualizer visualizer;
        IDisposable visualizerObserver;
        WorkflowViewModel viewModel;
        IServiceProvider serviceProvider;

        public VisualizerDialogLauncher(InspectBuilder source, DialogTypeVisualizer visualizer, WorkflowViewModel viewModel)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            if (visualizer == null)
            {
                throw new ArgumentNullException("visualizer");
            }

            if (viewModel == null)
            {
                throw new ArgumentNullException("viewModel");
            }

            this.source = source;
            this.visualizer = visualizer;
            this.viewModel = viewModel;
        }

        public string Text { get; set; }

        public InspectBuilder Source
        {
            get { return source; }
        }

        public DialogTypeVisualizer Visualizer
        {
            get { return visualizer; }
        }

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            using (var visualizerContext = new ServiceContainer(provider))
            {
                serviceProvider = provider;
                visualizerDialog.Text = Text;
                visualizerContext.AddService(typeof(ITypeVisualizerContext), this);
                visualizerContext.AddService(typeof(TypeVisualizerDialog), visualizerDialog);
                visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
                visualizerContext.AddService(typeof(ExpressionBuilderGraph), viewModel.Workflow);
                visualizer.Load(visualizerContext);
                var visualizerOutput = visualizer.Visualize(source.Output, visualizerContext);
                visualizerContext.RemoveService(typeof(ExpressionBuilderGraph));
                visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));
                visualizerContext.RemoveService(typeof(TypeVisualizerDialog));
                visualizerContext.RemoveService(typeof(ITypeVisualizerContext));

                visualizerDialog.AllowDrop = true;
                visualizerDialog.KeyPreview = true;
                visualizerDialog.KeyDown += new KeyEventHandler(visualizerDialog_KeyDown);
                visualizerDialog.DragEnter += new DragEventHandler(visualizerDialog_DragEnter);
                visualizerDialog.DragOver += new DragEventHandler(visualizerDialog_DragOver);
                visualizerDialog.DragDrop += new DragEventHandler(visualizerDialog_DragDrop);
                visualizerDialog.Load += delegate
                {
                    visualizerObserver = visualizerOutput.Subscribe();
                };

                visualizerDialog.FormClosing += delegate { visualizerObserver.Dispose(); visualizerObserver = null; };
                visualizerDialog.FormClosed += delegate { visualizer.Unload(); };
            }
        }

        void visualizerDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && e.Control)
            {
                var dialogMashup = visualizer as DialogMashupVisualizer;
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
            var dialogMashup = visualizer as DialogMashupVisualizer;
            if (visualizerObserver != null && dialogMashup != null)
            {
                dialogMashup.LoadMashups(serviceProvider);
                var visualizerOutput = visualizer.Visualize(source.Output, serviceProvider);
                visualizerObserver = visualizerOutput.Subscribe();
            }
        }

        void UnloadMashups()
        {
            var dialogMashup = visualizer as DialogMashupVisualizer;
            if (visualizerObserver != null && dialogMashup != null)
            {
                visualizerObserver.Dispose();
                dialogMashup.UnloadMashups();
            }
        }

        public void CreateMashup(GraphNode graphNode, IWorkflowEditorService editorService)
        {
            var dialogMashup = visualizer as DialogMashupVisualizer;
            var visualizerDialog = viewModel.GetVisualizerDialogLauncher(graphNode);
            if (dialogMashup != null && visualizerDialog != null)
            {
                var mashup = typeof(VisualizerMashup<,>).MakeGenericType(visualizer.GetType(), visualizerDialog.Visualizer.GetType());
                var mashupVisualizer = editorService.GetTypeVisualizer(mashup);
                if (mashupVisualizer != null)
                {
                    UnloadMashups();
                    var visualizerMashup = (DialogTypeVisualizer)Activator.CreateInstance(mashupVisualizer);
                    dialogMashup.Mashups.Add(new VisualizerMashup(visualizerDialog.Source.Output, visualizerMashup));
                    ReloadMashups();
                }
            }
        }

        void visualizerDialog_DragDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GraphNode)) && serviceProvider != null)
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var editorService = (IWorkflowEditorService)serviceProvider.GetService(typeof(IWorkflowEditorService));
                CreateMashup(graphNode, editorService);
            }
        }

        void visualizerDialog_DragOver(object sender, DragEventArgs e)
        {
        }

        void visualizerDialog_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(typeof(GraphNode)) && serviceProvider != null)
            {
                var graphViewSource = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var visualizerDialog = viewModel.GetVisualizerDialogLauncher(graphViewSource);
                var editorService = (IWorkflowEditorService)serviceProvider.GetService(typeof(IWorkflowEditorService));
                if (visualizerDialog != null)
                {
                    var mashup = typeof(VisualizerMashup<,>).MakeGenericType(visualizer.GetType(), visualizerDialog.Visualizer.GetType());
                    var mashupVisualizer = editorService.GetTypeVisualizer(mashup);
                    if (mashupVisualizer != null)
                    {
                        e.Effect = DragDropEffects.Link;
                    }
                }
            }
        }
    }
}
