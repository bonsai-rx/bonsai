using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Combinator]
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Compares a template against overlapped regions in the input array.")]
    public class MatchTemplate
    {
        public MatchTemplate()
        {
            MatchingMethod = TemplateMatchingMethod.CorrelationCoefficientNormed;
        }

        [Description("Specifies the method used to compare the template with array regions.")]
        public TemplateMatchingMethod MatchingMethod { get; set; }

        public IObservable<TArray> Process<TArray, TTemplate>(IObservable<Tuple<TArray, TTemplate>> source)
            where TArray : Arr
            where TTemplate : Arr
        {
            var outputFactory = ArrFactory<TArray>.DefaultFactory;
            return source.Select(input =>
            {
                var image = input.Item1;
                var template = input.Item2;
                var outputSize = image.Size;
                var templateSize = template.Size;
                outputSize.Width -= templateSize.Width - 1;
                outputSize.Height -= templateSize.Height - 1;
                var output = outputFactory(outputSize, Depth.F32, 1);
                CV.MatchTemplate(image, template, output, MatchingMethod);
                return output;
            });
        }
    }
}
