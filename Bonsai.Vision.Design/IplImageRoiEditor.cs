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
using System.Collections.ObjectModel;
using System.Drawing;

namespace Bonsai.Vision.Design
{
    public abstract class IplImageRoiEditor : UITypeEditor
    {
        protected IplImageRoiEditor(RectangleSource source)
        {
            Source = source;
        }

        private RectangleSource Source { get; set; }

        protected enum RectangleSource
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
                var regions = (CvPoint[][])value;
                var propertyDescriptor = context.PropertyDescriptor;

                using (var visualizerDialog = new TypeVisualizerDialog())
                using (var imageControl = new IplImageRoiPicker())
                {
                    visualizerDialog.Text = propertyDescriptor.Name;
                    if (regions != null)
                    {
                        foreach (var region in regions) imageControl.Regions.Add(region);
                    }

                    imageControl.SelectedRegionChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, imageControl.Regions.ToArray());
                    visualizerDialog.AddControl(imageControl);
                    imageControl.PictureBox.DoubleClick += (sender, e) =>
                    {
                        if (imageControl.Image != null)
                        {
                            visualizerDialog.ClientSize = new Size(imageControl.Image.Width, imageControl.Image.Height);
                        }
                    };

                    var workflow = (ExpressionBuilderGraph)provider.GetService(typeof(ExpressionBuilderGraph));
                    if (workflow == null) return base.EditValue(context, provider, value);

                    var workflowNode = (from node in workflow
                                        let builder = node.Value as SelectBuilder
                                        where builder != null && builder.Transform == context.Instance
                                        select node)
                                        .FirstOrDefault();
                    if (workflowNode == null) return base.EditValue(context, provider, value);

                    IObservable<object> source;
                    switch (Source)
                    {
                        case RectangleSource.Input: source = ((InspectBuilder)workflow.Predecessors(workflowNode).First().Value).Output; break;
                        case RectangleSource.Output: source = ((InspectBuilder)workflow.Successors(workflowNode).First().Value).Output; break;
                        default: return base.EditValue(context, provider, value);
                    }

                    IDisposable subscription = null;
                    imageControl.HandleCreated += delegate { subscription = source.ObserveOn(imageControl).Subscribe(image => imageControl.Image = (IplImage)image); };
                    imageControl.HandleDestroyed += delegate { subscription.Dispose(); };
                    editorService.ShowDialog(visualizerDialog);

                    return imageControl.Regions.ToArray();
                }
            }

            return base.EditValue(context, provider, value);
        }
    }
}
