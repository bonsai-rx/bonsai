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
using Bonsai.Expressions;
using Bonsai.Dag;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Vision.Design
{
    public abstract class IplImageQuadrangleEditor : DataSourceTypeEditor
    {
        protected IplImageQuadrangleEditor(DataSource source)
            : base(source, typeof(IplImage))
        {
        }

        protected virtual IObservable<IplImage> GetImageSource(IObservable<IObservable<object>> source)
        {
            return source.Merge().Select(image => image as IplImage);
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
                var quadrangle = ((Point2f[])value);
                if (quadrangle == null) return base.EditValue(context, provider, value);

                var propertyDescriptor = context.PropertyDescriptor;

                using (var visualizerDialog = new TypeVisualizerDialog())
                {
                    var imageControl = new ImageQuadranglePicker();
                    imageControl.Dock = DockStyle.Fill;
                    visualizerDialog.Text = propertyDescriptor.Name;
                    Array.Copy(quadrangle, imageControl.Quadrangle, imageControl.Quadrangle.Length);
                    imageControl.QuadrangleChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, imageControl.Quadrangle.Clone());
                    visualizerDialog.AddControl(imageControl);
                    imageControl.Canvas.DoubleClick += (sender, e) =>
                    {
                        if (imageControl.Image != null)
                        {
                            visualizerDialog.ClientSize = new System.Drawing.Size(
                                imageControl.Image.Width,
                                imageControl.Image.Height);
                        }
                    };

                    IDisposable subscription = null;
                    var source = GetDataSource(context, provider);
                    var imageSource = GetImageSource(source.Output);
                    imageControl.Load += delegate { subscription = imageSource.Subscribe(image => imageControl.Image = image as IplImage); };
                    imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                    editorService.ShowDialog(visualizerDialog);
                    return imageControl.Quadrangle;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
