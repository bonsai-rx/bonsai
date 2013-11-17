using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;
using System.Drawing.Design;

namespace Bonsai.Vision
{
    public class SelectChannel : Transform<IplImage, IplImage>
    {
        [Range(0, 3)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int Channel { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var output = new IplImage(input.Size, input.Depth, 1);
                var channel = Channel;
                if (channel < 0 || channel >= input.Channels) output.SetZero();
                else
                {
                    switch (channel)
                    {
                        case 0: CV.Split(input, output, null, null, null); break;
                        case 1: CV.Split(input, null, output, null, null); break;
                        case 2: CV.Split(input, null, null, output, null); break;
                        case 3: CV.Split(input, null, null, null, output); break;
                        default: throw new InvalidOperationException("Invalid channel number.");
                    }
                }

                return output;
            });
        }
    }
}
