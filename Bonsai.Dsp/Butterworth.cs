using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class Butterworth : Transform<Mat, Mat>
    {
        int filterOrder = 3;
        FilterType filterType;
        double cutoff1, cutoff2;
        double samplingFrequency;
        readonly IirFilter filter = new IirFilter();

        public double SamplingFrequency
        {
            get { return samplingFrequency; }
            set
            {
                samplingFrequency = value;
                UpdateFilter();
            }
        }

        public double Cutoff1
        {
            get { return cutoff1; }
            set
            {
                cutoff1 = value;
                UpdateFilter();
            }
        }

        public double Cutoff2
        {
            get { return cutoff2; }
            set
            {
                cutoff2 = value;
                UpdateFilter();
            }
        }

        public int FilterOrder
        {
            get { return filterOrder; }
            set
            {
                filterOrder = value;
                UpdateFilter();
            }
        }

        public FilterType FilterType
        {
            get { return filterType; }
            set
            {
                filterType = value;
                UpdateFilter();
            }
        }

        void UpdateFilter()
        {
            double[] b, a;
            var butter = FilterDesign.ButterworthPrototype(filterOrder);
            var fs = samplingFrequency;
            var cutoff1 = Cutoff1 / fs;
            var cutoff2 = Cutoff2 / fs;
            FilterDesign.GetFilterCoefficients(butter, new[] { cutoff1, cutoff2 }, filterType, out b, out a);
            filter.FeedforwardCoefficients = b;
            filter.FeedbackCoefficients = a;
        }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return filter.Process(source);
        }
    }
}
