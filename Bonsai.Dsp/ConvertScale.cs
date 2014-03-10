using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using OpenCV.Net;
using System.ComponentModel;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts the input array into the specified bit depth, with optional linear transformation.")]
    public class ConvertScale : ArrayTransform
    {
        public ConvertScale()
        {
            Scale = 1;
        }

        [TypeConverter(typeof(DepthConverter))]
        [Description("The target bit depth of individual array elements.")]
        public Depth? Depth { get; set; }

        [Description("The optional scale factor to apply to individual array elements.")]
        public double Scale { get; set; }

        [Description("The optional value to be added to individual array elements.")]
        public double Shift { get; set; }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateFactory;
            var outputDepthFactory = ArrFactory<TArray>.TemplateSizeChannelFactory;
            return source.Select(input =>
            {
                var depth = Depth;
                var output = depth.HasValue
                    ? outputDepthFactory(input, depth.Value)
                    : outputFactory(input);
                CV.ConvertScale(input, output, Scale, Shift);
                return output;
            });
        }

        class DepthConverter : EnumConverter
        {
            public DepthConverter()
                : base(typeof(Depth))
            {
            }

            public override TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
            {
                return new StandardValuesCollection(new[]
                {
                    OpenCV.Net.Depth.U8,
                    OpenCV.Net.Depth.S8,
                    OpenCV.Net.Depth.U16,
                    OpenCV.Net.Depth.S16,
                    OpenCV.Net.Depth.S32,
                    OpenCV.Net.Depth.F32,
                    OpenCV.Net.Depth.F64
                });
            }

            public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
            {
                return true;
            }
        }
    }
}
