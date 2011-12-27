using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using OpenCV.Net;
using Bonsai.Design;
using System.Windows.Forms;
using System.Drawing;
using System.Reactive.Disposables;

namespace Bonsai.Vision.Design
{
    public class HueSaturationHistogramEditor : UITypeEditor
    {
        const int Scale = 2;

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            var editorService = (IWindowsFormsEditorService)provider.GetService(typeof(IWindowsFormsEditorService));
            if (context != null && editorService != null)
            {
                var histogram = (CvHistogram)value;
                var project = (WorkflowProject)provider.GetService(typeof(WorkflowProject));
                if (!project.Running) return value;

                IplImage hue = null;
                IplImage saturation = null;
                IplImage rgb = null;
                var source = project.GetFilterInput(context.Instance);

                float min, max;
                int[] minIdx, maxIdx;
                minIdx = new int[2];
                maxIdx = new int[2];

                var pickerDialog = new TypeVisualizerDialog();
                var pickerControl = new IplImageRectanglePicker();
                var histogramImage = new IplImage(new CvSize(ColorBackProject.HueBins * Scale, ColorBackProject.SatBins * Scale), 8, 3);
                var histogramDialog = new TypeVisualizerDialog();
                var histogramControl = new IplImageControl();
                histogramControl.ClientSize = new Size(histogramImage.Width, histogramImage.Height);
                histogramControl.Image = histogramImage;
                histogramDialog.AddControl(histogramControl);

                pickerControl.PickedRectangleChanged += (sender, e) =>
                {
                    if (hue == null || saturation == null) return;

                    hue.ImageROI = pickerControl.PickedRectangle;
                    saturation.ImageROI = pickerControl.PickedRectangle;
                    ImgProc.cvCalcHist(new[] { hue, saturation }, histogram, 1, CvArr.Null);
                    hue.ResetImageROI();
                    saturation.ResetImageROI();

                    histogram.GetMinMaxHistValue(out min, out max, minIdx, maxIdx);
                    histogramImage.SetZero();

                    for (int h = 0; h < ColorBackProject.HueBins; h++)
                    {
                        for (int s = 0; s < ColorBackProject.SatBins; s++)
                        {
                            double binVal = histogram.QueryHistValue(h, s);
                            int intensity = (int)Math.Round(binVal * 255 / max);
                            Core.cvRectangle(histogramImage, new CvPoint(h * Scale, s * Scale),
                                         new CvPoint((h + 1) * Scale - 1, (s + 1) * Scale - 1),
                                         CvScalar.Rgb(intensity, intensity, intensity),
                                         -1, 8, 0);
                        }
                    }

                    histogramControl.Image = histogramImage;
                };
                pickerDialog.AddControl(pickerControl);
                Action<IplImage> sourceObserver = image =>
                {
                    if (hue == null)
                    {
                        hue = new IplImage(image.Size, 8, 1);
                        saturation = new IplImage(image.Size, 8, 1);
                        rgb = new IplImage(image.Size, 8, 3);
                    }

                    Core.cvSplit(image, hue, saturation, CvArr.Null, CvArr.Null);
                    ImgProc.cvCvtColor(image, rgb, ColorConversion.HSV2BGR);
                    pickerControl.Image = rgb;
                };

                histogramDialog.Show();
                var handler = (IDisposable)DynamicObservable.Subscribe(DynamicObservable.ObserveOn(source, pickerControl), sourceObserver);
                var disposable = new CompositeDisposable();
                disposable.Add(pickerDialog);
                disposable.Add(pickerControl);
                disposable.Add(histogramDialog);
                disposable.Add(histogramControl);
                disposable.Add(histogramImage);
                disposable.Add(handler);
                pickerDialog.FormClosed += delegate { disposable.Dispose(); };

                pickerDialog.Show();
            }

            return value;
        }
    }
}
