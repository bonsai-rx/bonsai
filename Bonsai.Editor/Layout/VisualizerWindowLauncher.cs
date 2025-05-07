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
    class VisualizerWindowLauncher : WindowLauncher, ITypeVisualizerContext
    {
        ServiceContainer visualizerContext;
        IDisposable visualizerObserver;

        public VisualizerWindowLauncher(
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

        public VisualizerFactory VisualizerFactory { get; }

        static IDisposable SubscribeWindow<TSource>(IObservable<TSource> source, TypeVisualizerWindow visualizerWindow)
        {
            return source.Subscribe(
                xs => { },
                ex => visualizerWindow.BeginInvoke(() =>
                {
                    MessageBox.Show(visualizerWindow, ex.Message, visualizerWindow.Text);
                    visualizerWindow.Close();
                }));
        }

        protected override void InitializeComponents(TypeVisualizerWindow visualizerWindow, IServiceProvider provider)
        {
            visualizerWindow.Text = Text;
            visualizerContext = new ServiceContainer(provider);
            visualizerContext.AddService(typeof(ITypeVisualizerContext), this);
            visualizerContext.AddService(typeof(TypeVisualizerWindow), visualizerWindow);
            visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerWindow);
            visualizerContext.AddService(typeof(ExpressionBuilderGraph), Workflow);
            Visualizer.Value.Load(visualizerContext);
            var visualizerOutput = Visualizer.Value.Visualize(VisualizerFactory.Source.Output, visualizerContext);

            visualizerWindow.AllowDrop = true;
            visualizerWindow.KeyPreview = true;
            visualizerWindow.KeyDown += new KeyEventHandler(visualizerWindow_KeyDown);
            visualizerWindow.DragEnter += new DragEventHandler(visualizerWindow_DragEnter);
            visualizerWindow.DragOver += new DragEventHandler(visualizerWindow_DragOver);
            visualizerWindow.DragDrop += new DragEventHandler(visualizerWindow_DragDrop);
            visualizerWindow.Load += (sender, e) => visualizerObserver = SubscribeWindow(visualizerOutput, visualizerWindow);

            visualizerWindow.HandleDestroyed += delegate
            {
                if (visualizerObserver != null)
                {
                    visualizerObserver.Dispose();
                    visualizerObserver = null;
                }
            };

            visualizerWindow.FormClosed += delegate
            {
                Visualizer.Value.Unload();
                visualizerContext.RemoveService(typeof(ExpressionBuilderGraph));
                visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));
                visualizerContext.RemoveService(typeof(TypeVisualizerWindow));
                visualizerContext.RemoveService(typeof(ITypeVisualizerContext));
                visualizerContext.Dispose();
                visualizerContext = null;
            };
        }

        void visualizerWindow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Back && e.Control)
            {
                var editorService = (IWorkflowEditorService)visualizerContext.GetService(typeof(IWorkflowEditorService));
                editorService.SelectBuilderNode(Source);
            }

            if (e.KeyCode == Keys.Delete && e.Control)
            {
                VisualizerWindow.BeginInvoke((Action)(() =>
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
            if (visualizerObserver != null && Visualizer.Value is MashupVisualizer windowMashup)
            {
                windowMashup.LoadMashups(visualizerContext);
                var visualizerOutput = windowMashup.Visualize(VisualizerFactory.Source.Output, visualizerContext);
                visualizerObserver = SubscribeWindow(visualizerOutput, VisualizerWindow);
            }
        }

        void UnloadMashups()
        {
            if (visualizerObserver != null && Visualizer.Value is MashupVisualizer windowMashup)
            {
                visualizerObserver.Dispose();
                windowMashup.UnloadMashups();
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

        public void CreateMashup(MashupVisualizer windowMashup, InspectBuilder source, Type visualizerType, TypeVisualizerMap typeVisualizerMap)
        {
            if (visualizerType == null)
                throw new ArgumentNullException(nameof(visualizerType));

            var windowMashupType = windowMashup.GetType();
            var mashupSourceType = LayoutHelper.GetMashupSourceType(windowMashupType, visualizerType, typeVisualizerMap);
            if (mashupSourceType != null)
            {
                UnloadMashups();
                if (mashupSourceType == typeof(DialogTypeVisualizer))
                {
                    mashupSourceType = visualizerType;
                }
                var visualizerMashup = (DialogTypeVisualizer)Activator.CreateInstance(mashupSourceType);
                windowMashup.MashupSources.Add(source, visualizerMashup);
                ReloadMashups();
            }
        }

        void visualizerWindow_DragDrop(object sender, DragEventArgs e)
        {
            if (visualizerContext != null && e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var inspectBuilder = (InspectBuilder)graphNode.Value;
                var typeVisualizerMap = (TypeVisualizerMap)visualizerContext.GetService(typeof(TypeVisualizerMap));
                var visualizerWindowMap = (VisualizerWindowMap)visualizerContext.GetService(typeof(VisualizerWindowMap));
                var visualizerType = GetVisualizerType(inspectBuilder, visualizerWindowMap, typeVisualizerMap);
                if (visualizerType != null)
                {
                    var visualizer = GetMashupContainer(e.X, e.Y);
                    CreateMashup(visualizer, inspectBuilder, visualizerType, typeVisualizerMap);
                }
            }
        }

        void visualizerWindow_DragOver(object sender, DragEventArgs e)
        {
        }

        void visualizerWindow_DragEnter(object sender, DragEventArgs e)
        {
            if (visualizerContext != null && e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var typeVisualizerMap = (TypeVisualizerMap)visualizerContext.GetService(typeof(TypeVisualizerMap));
                var visualizerWindowMap = (VisualizerWindowMap)visualizerContext.GetService(typeof(VisualizerWindowMap));
                var visualizerType = GetVisualizerType((InspectBuilder)graphNode.Value, visualizerWindowMap, typeVisualizerMap);
                if (visualizerType != null)
                {
                    var windowMashupType = VisualizerFactory.VisualizerType;
                    if (windowMashupType.IsSubclassOf(typeof(MashupVisualizer)))
                    {
                        var mashupVisualizerType = LayoutHelper.GetMashupSourceType(windowMashupType, visualizerType, typeVisualizerMap);
                        if (mashupVisualizerType != null)
                        {
                            e.Effect = DragDropEffects.Link;
                        }
                    }
                }
            }
        }

        Type GetVisualizerType(InspectBuilder source, VisualizerWindowMap visualizerWindowMap, TypeVisualizerMap typeVisualizerMap)
        {
            if (visualizerWindowMap.TryGetValue(source, out VisualizerWindowLauncher windowLauncher))
            {
                if (windowLauncher == this)
                    return null;
                else return windowLauncher.VisualizerFactory.VisualizerType;
            }
            else return typeVisualizerMap.GetTypeVisualizers(source).FirstOrDefault();
        }
    }
}
