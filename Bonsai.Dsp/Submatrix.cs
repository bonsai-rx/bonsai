using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that extracts a submatrix from each array in the sequence.
    /// </summary>
    [Description("Extracts a submatrix from each array in the sequence.")]
    public class Submatrix : Transform<Mat, Mat>
    {
        /// <summary>
        /// Gets or sets the first row of the submatrix.
        /// </summary>
        [Description("The first row of the submatrix.")]
        public int StartRow { get; set; }

        /// <summary>
        /// Gets or sets the last row of the submatrix. If it is not specified, the
        /// submatrix will end at the last row of the array.
        /// </summary>
        [Description("The optional last row of the submatrix.")]
        public int? EndRow { get; set; }

        /// <summary>
        /// Gets or sets the first column of the submatrix.
        /// </summary>
        [Description("The first column of the submatrix.")]
        public int StartCol { get; set; }

        /// <summary>
        /// Gets or sets the last column of the submatrix. If it is not specified, the
        /// submatrix will end at the last column of the array.
        /// </summary>
        [Description("The optional last column of the submatrix.")]
        public int? EndCol { get; set; }

        /// <summary>
        /// Extracts a submatrix from each array in an observable sequence.
        /// </summary>
        /// <param name="source">
        /// A sequence of 2D matrix values.
        /// </param>
        /// <returns>
        /// A sequence of 2D matrix values, where each matrix stores the range
        /// of rows and columns specified by the submatrix.
        /// </returns>
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
