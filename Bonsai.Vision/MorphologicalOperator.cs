using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Vision
{
    public class MorphologicalOperator : Transform<IplImage, IplImage>
    {
        Size size;
        Point anchor;
        StructuringElementShape shape;
        event EventHandler PropertyChanged;

        public MorphologicalOperator()
        {
            Size = new Size(3, 3);
            Anchor = new Point(1, 1);
            Iterations = 1;
        }

        public Size Size
        {
            get { return size; }
            set
            {
                size = value;
                OnPropertyChanged(EventArgs.Empty);
            }
        }

        public Point Anchor
        {
            get { return anchor; }
            set
            {
                anchor = value;
                OnPropertyChanged(EventArgs.Empty);
            }
        }

        public StructuringElementShape Shape
        {
            get { return shape; }
            set
            {
                shape = value;
                OnPropertyChanged(EventArgs.Empty);
            }
        }

        void OnPropertyChanged(EventArgs e)
        {
            var handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public int Iterations { get; set; }

        public OpenCV.Net.MorphologicalOperation Operation { get; set; }

        public override IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            var propertyChanged = Observable.FromEventPattern<EventArgs>(
                handler => PropertyChanged += new EventHandler(handler),
                handler => PropertyChanged -= new EventHandler(handler));

            return Observable.Defer(() =>
            {
                IplImage temp = null;
                IplConvKernel strel = null;
                bool updateStrel = false;
                var update = propertyChanged.Subscribe(xs => updateStrel = true);
                return source.Select(input =>
                {
                    if (strel == null || updateStrel)
                    {
                        updateStrel = false;
                        if (strel != null) strel.Close();
                        strel = new IplConvKernel(Size.Width, Size.Height, Anchor.X, Anchor.Y, Shape);
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    temp = IplImageHelper.EnsureImageFormat(temp, input.Size, input.Depth, input.Channels);
                    CV.MorphologyEx(input, output, temp, strel, Operation, Iterations);
                    return output;
                }).Finally(update.Dispose);
            });
        }
    }
}
