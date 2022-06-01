﻿using System;
using System.Linq;
using System.ComponentModel.Design;
using Bonsai.Expressions;
using System.Reactive.Linq;
using System.Windows.Forms;
using Bonsai.Editor.GraphView;
using Bonsai.Editor.GraphModel;

namespace Bonsai.Design
{
    class VisualizerDialogLauncher : DialogLauncher, ITypeVisualizerContext
    {
        readonly InspectBuilder visualizerSource;
        readonly ExpressionBuilderGraph workflow;
        readonly WorkflowGraphView graphView;
        ServiceContainer visualizerContext;
        IDisposable visualizerObserver;

        public VisualizerDialogLauncher(
            Type visualizerType,
            InspectBuilder visualizerElement,
            Func<DialogTypeVisualizer> visualizerFactory,
            ExpressionBuilderGraph visualizerWorkflow,
            InspectBuilder workflowSource,
            WorkflowGraphView workflowGraphView)
        {
            if (visualizerFactory == null)
            {
                throw new ArgumentNullException(nameof(visualizerFactory));
            }

            VisualizerType = visualizerType ?? throw new ArgumentNullException(nameof(visualizerType));
            visualizerSource = visualizerElement ?? throw new ArgumentNullException(nameof(visualizerElement));
            Visualizer = new Lazy<DialogTypeVisualizer>(visualizerFactory);
            Source = workflowSource ?? visualizerElement;
            workflow = visualizerWorkflow;
            graphView = workflowGraphView;
        }

        public string Text { get; set; }

        public InspectBuilder Source { get; }

        public Lazy<DialogTypeVisualizer> Visualizer { get; }

        public Type VisualizerType { get; }

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
            visualizerContext.AddService(typeof(ExpressionBuilderGraph), workflow);
            Visualizer.Value.Load(visualizerContext);
            var visualizerOutput = Visualizer.Value.Visualize(visualizerSource.Output, visualizerContext);

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

            if (graphView != null)
            {
                visualizerDialog.Shown += delegate
                {
                    visualizerDialog.Activated += delegate
                    {
                        graphView.SelectBuilderNode(Source);
                    };
                };
            }
        }

        void visualizerDialog_KeyDown(object sender, KeyEventArgs e)
        {
            if (graphView != null && e.KeyCode == Keys.Back && e.Control)
            {
                graphView.SelectBuilderNode(Source);
                graphView.EditorControl.ParentForm.Activate();
            }

            if (e.KeyCode == Keys.Delete && e.Control)
            {
                VisualizerDialog.BeginInvoke((Action)(() =>
                {
                    var mousePosition = Control.MousePosition;
                    var mashupContainer = GetMashupContainer(mousePosition.X, mousePosition.Y, allowEmpty: false);
                    if (mashupContainer != null && mashupContainer.Mashups.Count > 0)
                    {
                        UnloadMashups();
                        mashupContainer.Mashups.RemoveAt(mashupContainer.Mashups.Count - 1);
                        ReloadMashups();
                    }
                }));
            }
        }

        void ReloadMashups()
        {
            if (visualizerObserver != null && Visualizer.Value is DialogMashupVisualizer dialogMashup)
            {
                dialogMashup.LoadMashups(visualizerContext);
                var visualizerOutput = dialogMashup.Visualize(visualizerSource.Output, visualizerContext);
                visualizerObserver = SubscribeDialog(visualizerOutput, VisualizerDialog);
            }
        }

        void UnloadMashups()
        {
            if (visualizerObserver != null && Visualizer.Value is DialogMashupVisualizer dialogMashup)
            {
                visualizerObserver.Dispose();
                dialogMashup.UnloadMashups();
            }
        }

        DialogMashupVisualizer GetMashupContainer(int x, int y, bool allowEmpty = true)
        {
            var visualizer = Visualizer.Value as DialogMashupVisualizer;
            while (visualizer is MashupVisualizerContainer mashupContainer)
            {
                var mashup = mashupContainer.GetMashupAtPoint(x, y);
                if (mashup is MashupVisualizerAdapter mashupAdapter &&
                    mashupAdapter.Visualizer is DialogMashupVisualizer nestedMashup &&
                    (allowEmpty || nestedMashup.Mashups.Count > 0))
                {
                    visualizer = nestedMashup;
                }
                else break;
            }

            return visualizer;
        }

        static Type GetMashupVisualizerType(Type dialogMashupType, Type visualizerType, TypeVisualizerMap typeVisualizerMap)
        {
            var mashupVisualizerType = default(Type);
            while (dialogMashupType != null && dialogMashupType != typeof(DialogMashupVisualizer))
            {
                var mashup = typeof(VisualizerMashup<,>).MakeGenericType(dialogMashupType, visualizerType);
                mashupVisualizerType = typeVisualizerMap.GetTypeVisualizers(mashup).FirstOrDefault();
                if (mashupVisualizerType != null) break;

                mashup = typeof(VisualizerMashup<>).MakeGenericType(dialogMashupType);
                mashupVisualizerType = typeVisualizerMap.GetTypeVisualizers(mashup).FirstOrDefault();
                if (mashupVisualizerType != null) break;
                dialogMashupType = dialogMashupType.BaseType;
            }

            if (mashupVisualizerType.IsGenericTypeDefinition)
            {
                mashupVisualizerType = mashupVisualizerType.MakeGenericType(visualizerType);
            }
            return mashupVisualizerType;
        }

        public void CreateMashup(DialogMashupVisualizer dialogMashup, VisualizerDialogLauncher visualizerDialog, TypeVisualizerMap typeVisualizerMap)
        {
            if (visualizerDialog != null)
            {
                var dialogMashupType = dialogMashup.GetType();
                var visualizerType = visualizerDialog.VisualizerType;
                var mashupVisualizer = GetMashupVisualizerType(dialogMashupType, visualizerType, typeVisualizerMap);
                if (mashupVisualizer != null)
                {
                    UnloadMashups();
                    MashupTypeVisualizer visualizerMashup;
                    if (mashupVisualizer == typeof(MashupVisualizerAdapter))
                    {
                        var visualizer = (DialogTypeVisualizer)Activator.CreateInstance(visualizerType);
                        visualizerMashup = (MashupTypeVisualizer)Activator.CreateInstance(mashupVisualizer, visualizer);
                    }
                    else visualizerMashup = (MashupTypeVisualizer)Activator.CreateInstance(mashupVisualizer);
                    dialogMashup.Mashups.Add(new VisualizerMashup(visualizerDialog.visualizerSource, visualizerMashup));
                    ReloadMashups();
                }
            }
        }

        void visualizerDialog_DragDrop(object sender, DragEventArgs e)
        {
            if (graphView != null && visualizerContext != null && e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var typeVisualizerMap = (TypeVisualizerMap)visualizerContext.GetService(typeof(TypeVisualizerMap));
                var visualizerDialog = graphView.GetVisualizerDialogLauncher(graphNode);
                var visualizer = GetMashupContainer(e.X, e.Y);
                CreateMashup(visualizer, visualizerDialog, typeVisualizerMap);
            }
        }

        void visualizerDialog_DragOver(object sender, DragEventArgs e)
        {
        }

        void visualizerDialog_DragEnter(object sender, DragEventArgs e)
        {
            if (graphView != null && visualizerContext != null && e.Data.GetDataPresent(typeof(GraphNode)))
            {
                var graphNode = (GraphNode)e.Data.GetData(typeof(GraphNode));
                var visualizerDialog = graphView.GetVisualizerDialogLauncher(graphNode);
                if (visualizerDialog != null && visualizerDialog != this)
                {
                    var dialogMashupType = VisualizerType;
                    if (dialogMashupType.IsSubclassOf(typeof(DialogMashupVisualizer)))
                    {
                        var visualizerType = visualizerDialog.VisualizerType;
                        var typeVisualizerMap = (TypeVisualizerMap)visualizerContext.GetService(typeof(TypeVisualizerMap));
                        var mashupVisualizerType = GetMashupVisualizerType(dialogMashupType, visualizerType, typeVisualizerMap);
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
