using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Bonsai.Design;
using OpenCV.Net;
using System.Reflection;
using System.Reactive.Linq;

namespace Bonsai.Vision.Design
{
    public abstract class IplImageQuadrangleEditor : UITypeEditor
    {
        static readonly MethodInfo observeOn = typeof(ControlObservable).GetMethod("ObserveOn");
        static readonly MethodInfo subscribe = typeof(ObservableExtensions).GetMethods().First(m => m.Name == "Subscribe" && m.GetParameters().Length == 2);

        protected IplImageQuadrangleEditor(QuadrangleSource source)
        {
            Source = source;
        }

        private QuadrangleSource Source { get; set; }

        protected enum QuadrangleSource
        {
            Input,
            Output
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var quadrangle = ((CvPoint2D32f[])value);
                var propertyDescriptor = context.PropertyDescriptor;

                using (var visualizerDialog = new TypeVisualizerDialog())
                using (var imageControl = new IplImageQuadranglePicker())
                {
                    visualizerDialog.Text = propertyDescriptor.Name;
                    Array.Copy(quadrangle, imageControl.Quadrangle, imageControl.Quadrangle.Length);
                    imageControl.QuadrangleChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, imageControl.Quadrangle.Clone());
                    visualizerDialog.AddControl(imageControl);

                    var project = (WorkflowProject)provider.GetService(typeof(WorkflowProject));
                    object source;
                    switch (Source)
                    {
                        case QuadrangleSource.Input: source = project.GetFilterInput(context.Instance); break;
                        case QuadrangleSource.Output: source = project.GetFilterOutput(context.Instance); break;
                        default: return base.EditValue(context, provider, value); 
                    }

                    using (var handler = (IDisposable)DynamicObservable.Subscribe(DynamicObservable.ObserveOn(source, imageControl), (Action<IplImage>)(image => imageControl.Image = image)))
                    {
                        editorService.ShowDialog(visualizerDialog);
                    }

                    return imageControl.Quadrangle;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
