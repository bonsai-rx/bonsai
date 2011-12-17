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
        EventHandler visualizerHandler;

        public WorkflowElementControl()
        {
            InitializeComponent();
            Font = new Font(FontFamily.GenericMonospace, 8);
        }

        public bool Selected { get; set; }

        public AnchorStyles Connections { get; set; }

        public WorkflowElement Element { get; set; }

        public WorkflowElement ObservableElement { get; private set; }

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

            visualizerHandler = (sender, e) =>
            {
                if (visualizerDialog == null)
                {
                    using (var visualizerContext = new WorkflowContext(provider))
                    {
                        visualizerDialog = new TypeVisualizerDialog();
                        visualizerDialog.Text = Name;
                        visualizerContext.AddService(typeof(IDialogTypeVisualizerService), visualizerDialog);
                        visualizer.Load(visualizerContext);
                        visualizerDialog.FormClosed += delegate
                        {
                            visualizerObserver.Dispose();
                            visualizer.Unload();
                            visualizerDialog = null;
                        };

                        visualizerContext.RemoveService(typeof(IDialogTypeVisualizerService));
                        visualizerObserver = (IDisposable)subscribeMethod.Invoke(null, new object[] { observableSource, observer });
                        visualizerDialog.Show();
                    }
                }

                visualizerDialog.Focus();
            };

            ObservableElement = element;
        }

        private void WorkflowElementControl_Paint(object sender, PaintEventArgs e)
        {
            const float BorderSize = 5;
            const float ElementOffset = 25;
            var text = Element != null ? Element.GetType().Name.Substring(0, 1) : string.Empty;
            var textSize = e.Graphics.MeasureString(text, Font);

            var width = textSize.Width + 2 * BorderSize;
            var height = textSize.Height + 2 * BorderSize;
            if (Selected)
            {
                e.Graphics.FillRectangle(Brushes.Black, ElementOffset, ElementOffset, width, height);
                e.Graphics.DrawString(text, Font, Brushes.White, new PointF(ElementOffset + BorderSize, ElementOffset + BorderSize));
            }
            else
            {
                e.Graphics.DrawRectangle(Pens.Black, ElementOffset, ElementOffset, width, height);
                e.Graphics.DrawString(text, Font, Brushes.Black, new PointF(ElementOffset + BorderSize, ElementOffset + BorderSize));
            }

            var midX = ElementOffset + textSize.Width / 2f + BorderSize;
            var midY = ElementOffset + textSize.Height / 2f + BorderSize;
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
    }
}
