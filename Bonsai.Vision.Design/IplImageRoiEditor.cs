using System;
using System.Linq;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Bonsai.Design;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides an abstract base class for user interface editors that allow
    /// visually editing a collection of polygonal regions on top of the active
    /// image source.
    /// </summary>
    public abstract class IplImageRoiEditor : DataSourceTypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageRoiEditor"/> class
        /// using the specified image data source.
        /// </summary>
        /// <param name="source">
        /// Specifies the source of image notifications to the property editor.
        /// </param>
        protected IplImageRoiEditor(DataSource source)
            : base(source, typeof(IplImage))
        {
        }

        internal bool LabelRegions { get; set; }

        /// <summary>
        /// Gets the sequence of images arriving to or from the operator.
        /// </summary>
        /// <param name="source">
        /// An observable sequence that multicasts notifications from all the active
        /// subscriptions to the workflow operator.
        /// </param>
        protected virtual IObservable<IplImage> GetImageSource(IObservable<IObservable<object>> source)
        {
            return source.Merge().Select(image => image as IplImage);
        }

        /// <inheritdoc/>
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        /// <inheritdoc/>
        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                using (var visualizerDialog = new TypeVisualizerDialog())
                {
                    var imageControl = new ImageRoiPicker();
                    imageControl.LabelRegions = LabelRegions;
                    var regions = default(Point[][]);
                    var propertyDescriptor = context.PropertyDescriptor;
                    if (propertyDescriptor.PropertyType == typeof(Point[]))
                    {
                        if (value != null) regions = new[] { (Point[])value };
                        imageControl.MaxRegions = 1;
                    }
                    else regions = (Point[][])value;

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
                    var source = GetDataSource(context, provider);
                    var imageSource = GetImageSource(source.Output);
                    imageControl.Load += delegate { subscription = imageSource.Subscribe(image => imageControl.Image = image); };
                    imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                    editorService.ShowDialog(visualizerDialog);

                    var result = imageControl.Regions.ToArray();
                    if (result.Length == 0) return null;
                    else if (propertyDescriptor.PropertyType == typeof(Point[]))
                    {
                        return result[0];
                    }
                    else return result;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
