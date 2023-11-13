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
    /// Provides a user interface for visually editing circular regions on top of
    /// the input image sequence.
    /// </summary>
    public class IplImageCircleEditor : DataSourceTypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageCircleEditor"/> class
        /// using the input image data source.
        /// </summary>
        public IplImageCircleEditor()
            : this(DataSource.Input)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageCircleEditor"/> class
        /// using the specified image data source.
        /// </summary>
        /// <param name="source">
        /// Specifies the source of image notifications to the property editor.
        /// </param>
        protected IplImageCircleEditor(DataSource source)
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
                    var imageControl = new ImageEllipsePicker { IsCirclePicker = true };
                    var propertyDescriptor = context.PropertyDescriptor;
                    var singleRegion = propertyDescriptor.PropertyType == typeof(Circle);
                    if (singleRegion)
                    {
                        if (value is Circle circle && circle.Radius > 0)
                        {
                            regions = new[] { FromCircle(circle) };
                        }
                        imageControl.MaxRegions = 1;
                        imageControl.LabelRegions = false;
                    }
                    else if (value != null)
                    {
                        regions = Array.ConvertAll((Circle[])value, FromCircle);
                    }

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

        static RotatedRect FromCircle(Circle circle)
        {
            RotatedRect ellipse;
            ellipse.Angle = 0;
            ellipse.Center = circle.Center;
            ellipse.Size = new Size2f(circle.Radius * 2, circle.Radius * 2);
            return ellipse;
        }

        static Circle ToCircle(RotatedRect ellipse)
        {
            Circle circle;
            circle.Center = ellipse.Center;
            circle.Radius = ellipse.Size.Width / 2;
            return circle;
        }

        static object GetResult(IList<RotatedRect> regions, bool singleRegion)
        {
            if (regions.Count == 0) return null;
            else if (singleRegion) return ToCircle(regions[0]);
            else return regions.Select(ToCircle).ToArray();
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing circular regions on top of
    /// the output image sequence.
    /// </summary>
    public class IplImageOutputCircleEditor : IplImageCircleEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageOutputCircleEditor"/> class.
        /// </summary>
        public IplImageOutputCircleEditor()
            : base(DataSource.Output)
        {
        }
    }
}
