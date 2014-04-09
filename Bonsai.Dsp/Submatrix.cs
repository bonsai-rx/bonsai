using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Dsp
{
    public class Submatrix : Transform<Mat, Mat>
    {
        public int StartRow { get; set; }

        public int? EndRow { get; set; }

        public int StartCol { get; set; }

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
