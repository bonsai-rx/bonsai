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
    /// Provides a user interface for visually editing polygonal regions on top of
    /// the input image sequence.
    /// </summary>
    public class IplImageRoiEditor : DataSourceTypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageRoiEditor"/> class.
        /// </summary>
        public IplImageRoiEditor()
            : this(DataSource.Input)
        {
        }

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
                using (var visualizerDialog = new TypeVisualizerWindow())
                {
                    Point[][] regions = default;
                    var imageControl = new ImageRoiPicker();
                    imageControl.LabelRegions = LabelRegions;
                    var propertyDescriptor = context.PropertyDescriptor;
                    var singleRegion = propertyDescriptor.PropertyType == typeof(Point[]);
                    if (singleRegion)
                    {
                        if (value != null) regions = new[] { (Point[])value };
                        imageControl.MaxRegions = 1;
                    }
                    else regions = (Point[][])value;

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

        static object GetResult(IList<Point[]> regions, bool singleRegion)
        {
            if (regions.Count == 0) return null;
            else if (singleRegion) return regions[0];
            else return regions.ToArray();
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing polygonal regions on top of
    /// the output image sequence.
    /// </summary>
    public class IplImageOutputRoiEditor : IplImageRoiEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageOutputRoiEditor"/> class.
        /// </summary>
        public IplImageOutputRoiEditor()
            : base(DataSource.Output)
        {
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing labeled polygonal regions on
    /// top of the input image sequence.
    /// </summary>
    public class IplImageLabeledRoiEditor : IplImageRoiEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageLabeledRoiEditor"/> class.
        /// </summary>
        public IplImageLabeledRoiEditor()
            : base(DataSource.Input)
        {
            LabelRegions = true;
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing a collection of polygonal
    /// regions on top of the input image sequence.
    /// </summary>
    [Obsolete]
    public class IplImageInputRoiEditor : IplImageRoiEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageInputRoiEditor"/> class.
        /// </summary>
        public IplImageInputRoiEditor()
            : base(DataSource.Input)
        {
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing a collection of labeled
    /// polygonal regions on top of the active image source.
    /// </summary>
    [Obsolete]
    public class IplImageInputLabeledRoiEditor : IplImageRoiEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageInputLabeledRoiEditor"/> class.
        /// </summary>
        public IplImageInputLabeledRoiEditor()
            : base(DataSource.Input)
        {
            LabelRegions = true;
        }
    }
}
