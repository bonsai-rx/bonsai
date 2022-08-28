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
    /// Provides a user interface for visually editing a rectangular region on top
    /// of the input image sequence.
    /// </summary>
    public class IplImageRectangleEditor : DataSourceTypeEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageRectangleEditor"/> class.
        /// </summary>
        public IplImageRectangleEditor()
            : this(DataSource.Input)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageRectangleEditor"/> class
        /// using the specified image data source.
        /// </summary>
        /// <param name="source">
        /// Specifies the source of image notifications to the property editor.
        /// </param>
        protected IplImageRectangleEditor(DataSource source)
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
                var rectangle = (Rect)value;
                var propertyDescriptor = context.PropertyDescriptor;

                using (var visualizerDialog = new TypeVisualizerDialog())
                {
                    var imageControl = new ImageRectanglePicker();
                    imageControl.Dock = DockStyle.Fill;
                    visualizerDialog.Text = propertyDescriptor.Name;
                    imageControl.Rectangle = rectangle;
                    imageControl.RectangleChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, imageControl.Rectangle);
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
                    return imageControl.Rectangle;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing a rectangular region on top
    /// of the output image sequence.
    /// </summary>
    public class IplImageOutputRectangleEditor : IplImageRectangleEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageOutputRectangleEditor"/> class.
        /// </summary>
        public IplImageOutputRectangleEditor()
            : base(DataSource.Output)
        {
        }
    }

    /// <summary>
    /// Provides a user interface for visually editing a rectangular region on top
    /// of the input image sequence.
    /// </summary>
    [Obsolete]
    public class IplImageInputRectangleEditor : IplImageRectangleEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="IplImageInputRectangleEditor"/> class.
        /// </summary>
        public IplImageInputRectangleEditor()
            : base(DataSource.Input)
        {
        }
    }
}
