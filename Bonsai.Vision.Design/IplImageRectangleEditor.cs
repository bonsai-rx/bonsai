﻿using System;
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
    public abstract class IplImageRectangleEditor : DataSourceTypeEditor
    {
        protected IplImageRectangleEditor(DataSource source)
            : base(source)
        {
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
                var rectangle = (Rect)value;
                var propertyDescriptor = context.PropertyDescriptor;

                using (var visualizerDialog = new TypeVisualizerDialog())
                using (var imageControl = new ImageRectanglePicker())
                {
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
                    var source = GetDataSource(context, provider).Output.Merge();
                    imageControl.Load += delegate { subscription = source.Subscribe(image => imageControl.Image = (IplImage)image); };
                    imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                    editorService.ShowDialog(visualizerDialog);
                    return imageControl.Rectangle;
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
