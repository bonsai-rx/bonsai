using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    [Description("Repeats the input array in the horizontal or vertical dimensions.")]
    public class Tile : ArrayTransform
    {
        public Tile()
        {
            RowTiles = 1;
            ColumnTiles = 1;
        }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The number of times to repeat the input array in the vertical dimension.")]
        public int RowTiles { get; set; }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        [Description("The number of times to repeat the input array in the horizontal dimension.")]
        public int ColumnTiles { get; set; }

        public override IObservable<TArray> Process<TArray>(IObservable<TArray> source)
        {
            var outputFactory = ArrFactory<TArray>.TemplateDepthChannelFactory;
            return source.Select(input =>
            {
                var size = input.Size;
                var output = outputFactory(input, new Size(ColumnTiles * size.Width, RowTiles * size.Height));
                CV.Repeat(input, output);
                return output;
            });
        }
    }
}
