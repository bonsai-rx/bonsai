using System;
using System.Linq;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using Bonsai.Design;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Windows.Forms;
using System.Collections.Generic;

namespace Bonsai.Vision.Design
{
    /// <summary>
    /// Provides a user interface for visually editing elliptical regions on top of
    /// the input image sequence.
    /// </summary>
    public class IplImageEllipseEditor : DataSourceTypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageEllipseEditor"/> class
        /// using the input image data source.
        /// </summary>
        public IplImageEllipseEditor()
            : this(DataSource.Input)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageEllipseEditor"/> class
        /// using the specified image data source.
        /// </summary>
        /// <param name="source">
        /// Specifies the source of image notifications to the property editor.
        /// </param>
        protected IplImageEllipseEditor(DataSource source)
            : base(source, typeof(IplImage))
        {
        }

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
                    RotatedRect[] regions = default;
                    var imageControl = new ImageEllipsePicker();
                    var propertyDescriptor = context.PropertyDescriptor;
                    var singleRegion = propertyDescriptor.PropertyType == typeof(RotatedRect);
                    if (singleRegion)
                    {
                        if (value is RotatedRect ellipse &&
                            ellipse.Size.Width > 0 &&
                            ellipse.Size.Height > 0)
                        {
                            regions = new[] { ellipse };
                        }
                        imageControl.MaxRegions = 1;
                    }
                    else regions = (RotatedRect[])value;

                    imageControl.Dock = DockStyle.Fill;
                    visualizerDialog.Text = propertyDescriptor.Name;
                    if (regions != null)
                    {
                        foreach (var region in regions)
                        {
                            imageControl.Regions.Add(region);
                        }
                    }

                    imageControl.RegionsChanged += (sender, e) =>
                    {
                        propertyDescriptor.SetValue(context.Instance, GetResult(imageControl.Regions, singleRegion));
                    };

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
                    return GetResult(imageControl.Regions, singleRegion);
                }
            }

            return base.EditValue(context, provider, value);
        }

        static object GetResult(IList<RotatedRect> regions, bool singleRegion)
        {
            if (regions.Count == 0) return null;
            else if (singleRegion) return regions[0];
            else return regions.ToArray();
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing elliptical regions on top of
    /// the output image sequence.
    /// </summary>
    public class IplImageOutputEllipseEditor : IplImageEllipseEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageOutputEllipseEditor"/> class.
        /// </summary>
        public IplImageOutputEllipseEditor()
            : base(DataSource.Output)
        {
        }
    }

    /// <summary>
    /// Provides an abstract base class for user interface editors that allow
    /// visually editing a collection of elliptical regions on top of the active
    /// image source.
    /// </summary>
    [Obsolete]
    public abstract class IplImageEllipseRoiEditor : IplImageEllipseEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageEllipseRoiEditor"/> class
        /// using the specified image data source.
        /// </summary>
        /// <param name="source">
        /// Specifies the source of image notifications to the property editor.
        /// </param>
        protected IplImageEllipseRoiEditor(DataSource source)
            : base(source)
        {
        }
    }
}
