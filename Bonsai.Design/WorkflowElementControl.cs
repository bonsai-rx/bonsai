using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Linq.Expressions;

namespace Bonsai.Design
{
    public partial class WorkflowElementControl : UserControl
    {
        bool selected;
        EventHandler visualizerHandler;

        public WorkflowElementControl()
        {
            InitializeComponent();
            Dock = DockStyle.Fill;
            Font = new Font(FontFamily.GenericMonospace, 8);
        }

        public AnchorStyles Connections { get; set; }

        public WorkflowElement Element { get; set; }

        public WorkflowElement ObservableElement { get; private set; }

        public void SetObservableElement(WorkflowElement element, DialogTypeVisualizer visualizer, IServiceProvider provider)
        {
            IDisposable visualizerObserver = null;
            TypeVisualizerDialog visualizerDialog = null;
            var outputType = GetWorkflowElementOutputType(element);

            var input = Expression.Parameter(outputType);
            var output = Expression.Call(Expression.Constant(visualizer), typeof(DialogTypeVisualizer).GetMethod("Show"), input);
            var observer = Expression.Lambda(output, input).Compile();
            var outputProperty = element.GetType().GetProperty("Output");
            var observableSource = outputProperty.GetValue(element, null);
            var subscribeMethod = typeof(ObservableExtensions).GetMethods().First(m => m.Name == "Subscribe" && m.GetParameters().Length == 2);
            subscribeMethod = subscribeMethod.MakeGenericMethod(new[] { outputType });

            WorkflowContext context = null;
            EventHandler<LoadUnloadEventArgs> loadedHandler = (sender, e) => context = e.Context;
            element.Loaded += loadedHandler;
            Disposed += (sender, e) => element.Loaded -= loadedHandler;

            visualizerHandler = (sender, e) =>
            {
                if (visualizerDialog == null)
                {
                    if (context == null) return;
                    var workflow = (Workflow)context.GetService(typeof(Workflow));
                    if (workflow == null || !workflow.Running) return;

                    using (var visualizerContext = new WorkflowContext(context))
                    {
                        visualizerDialog = new TypeVisualizerDialog();
                        visualizerDialog.Text = Name;
                        visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
                        visualizer.Load(visualizerContext);

                        EventHandler runningChangedHandler = delegate { if (!workflow.Running) visualizerDialog.Close(); };
                        workflow.RunningChanged += runningChangedHandler;

                        visualizerDialog.FormClosing += delegate { visualizerObserver.Dispose(); };
                        visualizerDialog.FormClosed += delegate
                        {
                            workflow.RunningChanged -= runningChangedHandler;
                            visualizer.Unload();
                            visualizerDialog = null;
                        };

                        visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));
                        visualizerObserver = (IDisposable)subscribeMethod.Invoke(null, new object[] { observableSource, observer });
                        visualizerDialog.Show();
                    }
                }

                visualizerDialog.Activate();
            };

            ObservableElement = element;
        }

        protected override void OnGotFocus(EventArgs e)
        {
            foreach (Control control in Parent.Controls)
            {
                var elementControl = control as WorkflowElementControl;
                if (elementControl != null && elementControl.selected)
                {
                    elementControl.selected = false;
                    elementControl.Invalidate();
                }
            }

            Invalidate();
            selected = true;
            base.OnGotFocus(e);
        }

        private void WorkflowElementControl_Paint(object sender, PaintEventArgs e)
        {
            const float BorderSize = 5;
            const float ElementOffset = 20;
            var text = Element != null ? Element.GetType().Name.Substring(0, 1) : string.Empty;
            var textSize = e.Graphics.MeasureString(text, Font);

            var width = textSize.Width + 2 * BorderSize;
            var height = textSize.Height + 2 * BorderSize;
            if (selected)
            {
                e.Graphics.FillRectangle(Brushes.Black, ElementOffset, ElementOffset, width, height);
                e.Graphics.DrawString(text, Font, Brushes.White, new PointF(ElementOffset + BorderSize, ElementOffset + BorderSize));
            }
            else
            {
                e.Graphics.DrawRectangle(Pens.Black, ElementOffset, ElementOffset, width, height);
                e.Graphics.DrawString(text, Font, Brushes.Black, new PointF(ElementOffset + BorderSize, ElementOffset + BorderSize));
            }

            var midX = ElementOffset + width / 2f;
            var midY = ElementOffset + height / 2f;
            if (Connections.HasFlag(AnchorStyles.Left)) e.Graphics.DrawLine(Pens.Black, 0, midY, ElementOffset, midY);
            if (Connections.HasFlag(AnchorStyles.Right)) e.Graphics.DrawLine(Pens.Black, ElementOffset + width, midY, Size.Width, midY);
            if (Connections.HasFlag(AnchorStyles.Top)) e.Graphics.DrawLine(Pens.Black, midX, 0, midX, ElementOffset);
            if (Connections.HasFlag(AnchorStyles.Bottom)) e.Graphics.DrawLine(Pens.Black, midX, ElementOffset + height, midX, Size.Height);
        }

        private void WorkflowElementControl_DoubleClick(object sender, EventArgs e)
        {
            var handler = visualizerHandler;
            if (handler != null)
            {
                handler(sender, e);
            }
        }

        public static WorkflowElementControl FromWorkflowElement(WorkflowElement element)
        {
            var type = element.GetType();
            var elementControl = new WorkflowElementControl();
            elementControl.Name = type.Name;
            elementControl.Element = element;

            if (MatchGenericType(type, typeof(Source<>))) elementControl.Connections = AnchorStyles.Right;
            else if (MatchGenericType(type, typeof(Filter<,>)))
            {
                elementControl.Connections = AnchorStyles.Left | AnchorStyles.Right;
                if (MatchGenericType(type, typeof(ParallelFilter<>))) elementControl.Connections |= AnchorStyles.Bottom;
            }
            else if (MatchGenericType(type, typeof(Sink<>))) elementControl.Connections = AnchorStyles.Left;
            else throw new ArgumentException("Invalid workflow element type.", "element");

            return elementControl;
        }

        public static Type GetWorkflowElementOutputType(WorkflowElement element)
        {
            if (element == null) throw new ArgumentNullException("element");

            var type = element.GetType();
            while (type != null)
            {
                if (type.IsGenericType)
                {
                    var arguments = type.GetGenericArguments();
                    return arguments[arguments.Length - 1];
                }

                type = type.BaseType;
            }

            return null;
        }

        public static bool MatchGenericType(Type type, Type genericType)
        {
            if (!genericType.IsGenericType)
            {
                throw new ArgumentException("Trying to match against a non-generic type.", "genericType");
            }

            while (type != null)
            {
                if (type.IsGenericType && type.GetGenericTypeDefinition() == genericType)
                {
                    return true;
                }

                type = type.BaseType;
            }

            return false;
        }
    }
}
