using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;

namespace Bonsai.Dsp.Design
{
    public class SpikeThresholdEditor : DataSourceTypeEditor
    {
        public SpikeThresholdEditor()
            : base(DataSource.Output)
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
                var propertyDescriptor = context.PropertyDescriptor;
                var source = GetDataSource(context, provider);
                var chart = new ChartPanel(source, provider);
                chart.ClientSize = new System.Drawing.Size(320, 320);
                var spikeVisualizer = new SpikeWaveformCollectionVisualizer<WaveformThresholdPicker>();
                spikeVisualizer.Load(chart);
                var thresholdPicker = spikeVisualizer.Graph;
                thresholdPicker.Threshold = (double[])value;
                thresholdPicker.ThresholdChanged += (sender, e) => propertyDescriptor.SetValue(context.Instance, thresholdPicker.Threshold);
                var visualizerObservable = spikeVisualizer.Visualize(source.Output, chart);
                chart.HandleCreated += delegate { subscription = visualizerObservable.Subscribe(); };
                chart.Leave += delegate { editorService.CloseDropDown(); subscription.Dispose(); };
                try
                {
                    editorService.DropDownControl(chart);
                }
                finally
                {
                    chart.Dispose();
                    spikeVisualizer.Unload();
                }

                return thresholdPicker.Threshold;
            }

            return base.EditValue(context, provider, value);
        }
    }
}
