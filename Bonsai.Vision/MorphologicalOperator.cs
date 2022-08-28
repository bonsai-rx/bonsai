using System;
using System.Linq;
using OpenCV.Net;
using System.Reactive.Linq;
using System.ComponentModel;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that applies a morphological transformation kernel to
    /// each image in the sequence.
    /// </summary>
    [Description("Applies a morphological transformation kernel to each image in the sequence.")]
    public class MorphologicalOperator : Transform<IplImage, IplImage>
    {
        Size size;
        Point anchor;
        StructuringElementShape shape;
        event EventHandler PropertyChanged;

        /// <summary>
        /// Initializes a new instance of the <see cref="MorphologicalOperator"/> class.
        /// </summary>
        public MorphologicalOperator()
        {
            Size = new Size(3, 3);
            Anchor = new Point(-1, -1);
            Iterations = 1;
        }

        /// <summary>
        /// Gets or sets the size of the structuring element.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the anchor of the structuring element.
        /// </summary>
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

        /// <summary>
        /// Gets or sets the shape of the structuring element.
        /// </summary>
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
            PropertyChanged?.Invoke(this, e);
        }

        /// <summary>
        /// Gets or sets the number of times to apply the morphological operator.
        /// </summary>
        [Range(0, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, DesignTypes.UITypeEditor)]
        [Description("The number of times to apply the morphological operator.")]
        public int Iterations { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the type of morphological operation
        /// to be applied.
        /// </summary>
        [Description("Specifies the type of morphological operation to be applied.")]
        public MorphologicalOperation Operation { get; set; }

        /// <summary>
        /// Applies a morphological transformation kernel to each image in an
        /// observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of images for which to apply the morphological operator.
        /// </param>
        /// <returns>
        /// The sequence of transformed images.
        /// </returns>
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

            public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
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
