using System;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Windows.Forms;
using Bonsai.Editor.GraphModel;
using System.Linq;
using Bonsai.Editor;

namespace Bonsai.Design
{
    class VisualizerDialogLauncher : DialogLauncher, ITypeVisualizerContext
    {
        ServiceContainer visualizerContext;
        IDisposable visualizerObserver;

        public VisualizerDialogLauncher(
            Lazy<DialogTypeVisualizer> visualizer,
            VisualizerFactory visualizerFactory,
            ExpressionBuilderGraph workflow,
            InspectBuilder workflowSource)
        {
            Visualizer = visualizer ?? throw new ArgumentNullException(nameof(visualizer));
            VisualizerFactory = visualizerFactory ?? throw new ArgumentNullException(nameof(visualizerFactory));
            Source = workflowSource ?? throw new ArgumentNullException(nameof(workflowSource));
            Workflow = workflow ?? throw new ArgumentNullException(nameof(workflow));
        }

        public string Text { get; set; }

        public InspectBuilder Source { get; }

        public ExpressionBuilderGraph Workflow { get; }

        public Lazy<DialogTypeVisualizer> Visualizer { get; }

        private VisualizerFactory VisualizerFactory { get; }

        static IDisposable SubscribeDialog<TSource>(IObservable<TSource> source, TypeVisualizerDialog visualizerDialog)
        {
            return source.Subscribe(
                xs => { },
                ex => visualizerDialog.BeginInvoke((Action)(() =>
                {
                    MessageBox.Show(visualizerDialog, ex.Message, visualizerDialog.Text);
                    visualizerDialog.Close();
                })));
        }

        protected override void InitializeComponents(TypeVisualizerDialog visualizerDialog, IServiceProvider provider)
        {
            visualizerDialog.Text = Text;
            visualizerContext = new ServiceContainer(provider);
            visualizerContext.AddService(typeof(ITypeVisualizerContext), this);
            visualizerContext.AddService(typeof(TypeVisualizerDialog), visualizerDialog);
            visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
            visualizerContext.AddService(typeof(ExpressionBuilderGraph), Workflow);
            Visualizer.Value.Load(visualizerContext);
            var visualizerOutput = Visualizer.Value.Visualize(VisualizerFactory.Source.Output, visualizerContext);

            visualizerDialog.AllowDrop = true;
            visualizerDialog.KeyPreview = true;
            visualizerDialog.KeyDown += new KeyEventHandler(visualizerDialog_KeyDown);
            visualizerDialog.DragEnter += new DragEventHandler(visualizerDialog_DragEnter);
            visualizerDialog.DragOver += new DragEventHandler(visualizerDialog_DragOver);
            visualizerDialog.DragDrop += new DragEventHandler(visualizerDialog_DragDrop);
            visualizerDialog.Load += (sender, e) => visualizerObserver = SubscribeDialog(visualizerOutput, visualizerDialog);

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
                Visualizer.Value.Unload();
                visualizerContext.RemoveService(typeof(ExpressionBuilderGraph));
                visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));
                visualizerContext.RemoveService(typeof(TypeVisualizerDialog));
                visualizerContext.RemoveService(typeof(ITypeVisualizerContext));
                visualizerContext.Dispose();
                visualizerContext = null;
            };
        }

        void visualizerDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && e.Control)
            {
                var editorService = (IWorkflowEditorService)visualizerContext.GetService(typeof(IWorkflowEditorService));
                editorService.SelectBuilderNode(Source);
            }

            if (e.KeyCode == Keys.Delete && e.Control)
            {
                VisualizerDialog.BeginInvoke((Action)(() =>
                {
                    var mousePosition = Control.MousePosition;
                    var mashupContainer = GetMashupContainer(mousePosition.X, mousePosition.Y, allowEmpty: false);
                    if (mashupContainer != null && mashupContainer.MashupSources.Count > 0)
                    {
                        UnloadMashups();
                        mashupContainer.MashupSources.RemoveAt(mashupContainer.MashupSources.Count - 1);
                        ReloadMashups();
                    }
                }));
            }
        }

        void ReloadMashups()
        {
            if (visualizerObserver != null && Visualizer.Value is MashupVisualizer dialogMashup)
            {
                dialogMashup.LoadMashups(visualizerContext);
                var visualizerOutput = dialogMashup.Visualize(VisualizerFactory.Source.Output, visualizerContext);
                visualizerObserver = SubscribeDialog(visualizerOutput, VisualizerDialog);
            }
        }

        void UnloadMashups()
        {
            if (visualizerObserver != null && Visualizer.Value is MashupVisualizer dialogMashup)
            {
                visualizerObserver.Dispose();
                dialogMashup.UnloadMashups();
            }
        }

        MashupVisualizer GetMashupContainer(int x, int y, bool allowEmpty = true)
        {
            var visualizer = Visualizer.Value as MashupVisualizer;
            while (visualizer is MashupVisualizer mashupContainer)
            {
                var source = mashupContainer.GetMashupSource(x, y);
                if (source?.Visualizer is MashupVisualizer nestedMashup &&
                    (allowEmpty || nestedMashup.MashupSources.Count > 0))
                {
                    visualizer = nestedMashup;
                }
                else break;
            }

            return visualizer;
        }

        public void CreateMashup(MashupVisualizer dialogMashup, InspectBuilder source, Type visualizerType, TypeVisualizerMap typeVisualizerMap)
        {
            if (visualizerType == null)
                throw new ArgumentNullException(nameof(visualizerType));

            var dialogMashupType = dialogMashup.GetType();
            var mashupSourceType = LayoutHelper.GetMashupSourceType(dialogMashupType, visualizerType, typeVisualizerMap);
            if (mashupSourceType != null)
            {
                UnloadMashups();
                if (mashupSourceType == typeof(DialogTypeVisualizer))
                {
                    mashupSourceType = visualizerType;
                }
                var visualizerMashup = (DialogTypeVisualizer)Activator.CreateInstance(mashupSourceType);
                dialogMashup.MashupSources.Add(source, visualizerMashup);
                ReloadMashups();
            }
        }

        void visualizerDialog_DragDrop(object sender, DragEventArgs e)
        {
            if (visualizerContext != null && e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var inspectBuilder = (InspectBuilder)graphNode.Value;
                var typeVisualizerMap = (TypeVisualizerMap)visualizerContext.GetService(typeof(TypeVisualizerMap));
                var visualizerDialogMap = (VisualizerDialogMap)visualizerContext.GetService(typeof(VisualizerDialogMap));
                var visualizerType = GetVisualizerType(inspectBuilder, visualizerDialogMap, typeVisualizerMap);
                if (visualizerType != null)
                {
                    var visualizer = GetMashupContainer(e.X, e.Y);
                    CreateMashup(visualizer, inspectBuilder, visualizerType, typeVisualizerMap);
                }
            }
        }

        void visualizerDialog_DragOver(object sender, DragEventArgs e)
        {
        }

        void visualizerDialog_DragEnter(object sender, DragEventArgs e)
        {
            if (visualizerContext != null && e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var typeVisualizerMap = (TypeVisualizerMap)visualizerContext.GetService(typeof(TypeVisualizerMap));
                var visualizerDialogMap = (VisualizerDialogMap)visualizerContext.GetService(typeof(VisualizerDialogMap));
                var visualizerType = GetVisualizerType((InspectBuilder)graphNode.Value, visualizerDialogMap, typeVisualizerMap);
                if (visualizerType != null)
                {
                    var dialogMashupType = VisualizerFactory.VisualizerType;
                    if (dialogMashupType.IsSubclassOf(typeof(MashupVisualizer)))
                    {
                        var mashupVisualizerType = LayoutHelper.GetMashupSourceType(dialogMashupType, visualizerType, typeVisualizerMap);
                        if (mashupVisualizerType != null)
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
            }
        }

        Type GetVisualizerType(InspectBuilder source, VisualizerDialogMap visualizerDialogMap, TypeVisualizerMap typeVisualizerMap)
        {
            if (visualizerDialogMap.TryGetValue(source, out VisualizerDialogLauncher dialogLauncher))
            {
                if (dialogLauncher == this)
                    return null;
                else return dialogLauncher.VisualizerFactory.VisualizerType;
            }
            else return typeVisualizerMap.GetTypeVisualizers(source).FirstOrDefault();
        }
    }
}
