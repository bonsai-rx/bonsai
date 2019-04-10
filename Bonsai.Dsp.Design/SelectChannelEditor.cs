using Bonsai.Design;
using Bonsai.Expressions;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Bonsai.Dsp.Design
{
    public class SelectChannelEditor : DataSourceTypeEditor
    {
        public SelectChannelEditor()
            : base(DataSource.Input, typeof(Mat))
        {
        }

        public override bool IsDropDownResizable
        {
            get { return true; }
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.DropDown;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (editorService != null)
            {
                IDisposable subscription = null;
                var source = GetDataSource(context, provider);
                var chart = new ChartPanel(source, provider);
                chart.ClientSize = new System.Drawing.Size(320, 180);
                var matVisualizer = new MatVisualizer();
                matVisualizer.OverlayChannels = false;
                matVisualizer.SelectedChannels = (int[])value;
                matVisualizer.Load(chart);
                var visualizerObservable = matVisualizer.Visualize(source.Output, chart);
                chart.HandleCreated += delegate { subscription = visualizerObservable.Subscribe(); };
                chart.Leave += delegate { editorService.CloseDropDown(); subscription.Dispose(); };
                try
                {
                    editorService.DropDownControl(chart);
                }
                finally
                {
                    chart.Dispose();
                    matVisualizer.Unload();
                }

                return matVisualizer.SelectedChannels;
            }

            return base.EditValue(context, provider, value);
        }
    }
}
