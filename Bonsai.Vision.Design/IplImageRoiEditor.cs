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
using System.Collections.ObjectModel;
using System.Drawing;
using System.Windows.Forms;

namespace Bonsai.Vision.Design
{
    public abstract class IplImageRoiEditor : DataSourceTypeEditor
    {
        protected IplImageRoiEditor(DataSource source)
            : base(source, typeof(IplImage))
        {
        }

        internal bool LabelRegions { get; set; }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                using (var visualizerDialog = new TypeVisualizerDialog())
                {
                    var imageControl = new ImageRoiPicker();
                    imageControl.LabelRegions = LabelRegions;
                    var regions = default(OpenCV.Net.Point[][]);
                    var propertyDescriptor = context.PropertyDescriptor;
                    if (propertyDescriptor.PropertyType == typeof(OpenCV.Net.Point[]))
                    {
                        if (value != null) regions = new[] { (OpenCV.Net.Point[])value };
                        imageControl.MaxRegions = 1;
                    }
                    else regions = (OpenCV.Net.Point[][])value;

                    imageControl.Dock = DockStyle.Fill;
                    visualizerDialog.Text = propertyDescriptor.Name;
                    if (regions != null)
                    {
                        foreach (var region in regions) imageControl.Regions.Add(region);
                    }

                    imageControl.RegionsChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, imageControl.Regions.ToArray());
                    visualizerDialog.AddControl(imageControl);
                    imageControl.Canvas.MouseDoubleClick += (sender, e) =>
                    {
                        if (e.Button == MouseButtons.Left && imageControl.Image != null && !imageControl.SelectedRegion.HasValue)
                        {
                            visualizerDialog.ClientSize = new System.Drawing.Size(
                                imageControl.Image.Width,
                                imageControl.Image.Height);
                        }
                    };

                    IDisposable subscription = null;
                    var source = GetDataSource(context, provider).Output.Merge();
                    imageControl.Load += delegate { subscription = source.Subscribe(image => imageControl.Image = image as IplImage); };
                    imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                    editorService.ShowDialog(visualizerDialog);

                    var result = imageControl.Regions.ToArray();
                    if (propertyDescriptor.PropertyType == typeof(OpenCV.Net.Point[]))
                    {
                        return result.Length > 0 ? result[0] : null;
                    }
                    else return result;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
