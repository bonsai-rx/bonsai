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
    [Description("Extracts a submatrix of the input array.")]
    public class Submatrix : Transform<Mat, Mat>
    {
        [Description("The first row of the submatrix.")]
        public int StartRow { get; set; }

        [Description("The optional last row of the submatrix.")]
        public int? EndRow { get; set; }

        [Description("The first column of the submatrix.")]
        public int StartCol { get; set; }

        [Description("The optional last column of the submatrix.")]
        public int? EndCol { get; set; }

        public override IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.Select(input =>
            {
                return input.GetRows(StartRow, EndRow.GetValueOrDefault(input.Rows))
                            .GetCols(StartCol, EndCol.GetValueOrDefault(input.Cols));
            });
        }
    }
}
