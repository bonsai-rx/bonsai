using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;
using System.ComponentModel;

namespace Bonsai.Vision
{
    [Description("Applies advanced morphological operators to the input image.")]
    public class MorphologicalOperator : Transform<IplImage, IplImage>
    {
        Size size;
        Point anchor;
        StructuringElementShape shape;
        event EventHandler PropertyChanged;

        public MorphologicalOperator()
        {
            Size = new Size(3, 3);
            Anchor = new Point(-1, -1);
            Iterations = 1;
        }

        [Description("The size of the structuring element.")]
        public Size Size
        {
            get { return size; }
            set
            {
                size = value;
                OnPropertyChanged(EventArgs.Empty);
            }
        }

        [Description("The anchor of the structuring element.")]
        public Point Anchor
        {
            get { return anchor; }
            set
            {
                anchor = value;
                OnPropertyChanged(EventArgs.Empty);
            }
        }

        [TypeConverter(typeof(ShapeConverter))]
        [Description("The shape of the structuring element.")]
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

        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of times to apply the morphological operator.")]
        public int Iterations { get; set; }

        [Description("The type of morphological operation to be applied.")]
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
                        var size = Size;
                        var anchor = Anchor;
                        updateStrel = false;
                        if (strel != null) strel.Close();
                        strel = new IplConvKernel(
                            size.Width,
                            size.Height,
                            anchor.X < 0 ? size.Width / 2 : anchor.X,
                            anchor.Y < 0 ? size.Height / 2 : anchor.Y,
                            Shape);
                    }

                    var output = new IplImage(input.Size, input.Depth, input.Channels);
                    temp = IplImageHelper.EnsureImageFormat(temp, input.Size, input.Depth, input.Channels);
                    CV.MorphologyEx(input, output, temp, strel, Operation, Iterations);
                    return output;
                }).Finally(update.Dispose);
            });
        }

        class ShapeConverter : EnumConverter
        {
            internal ShapeConverter()
                : base(typeof(StructuringElementShape))
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[]
                {
                    StructuringElementShape.Rectangle,
                    StructuringElementShape.Cross,
                    StructuringElementShape.Ellipse
                });
            }
        }
    }
}
