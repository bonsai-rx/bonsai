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
    public class Tile : ArrayTransform
    {
        public Tile()
        {
            RowTiles = 1;
            ColumnTiles = 1;
        }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
        public int RowTiles { get; set; }

        [Range(1, int.MaxValue)]
        [Editor(DesignTypes.NumericUpDownEditor, typeof(UITypeEditor))]
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
