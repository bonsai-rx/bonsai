using OpenCV.Net;
using System;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;

namespace Bonsai.Vision
{
    /// <summary>
    /// Represents an operator that finds the positions of internal corners for
    /// each chessboard image in the sequence.
    /// </summary>
    [Description("Finds the positions of internal corners for each chessboard image in the sequence.")]
    public class FindChessboardCorners : Transform<IplImage, KeyPointCollection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FindChessboardCorners"/> class.
        /// </summary>
        public FindChessboardCorners()
        {
            PatternSize = new Size(9, 7);
            CalibrationFlags = ChessboardCalibrationFlags.AdaptiveThreshold
                | ChessboardCalibrationFlags.NormalizeImage
                | ChessboardCalibrationFlags.FastCheck;
        }

        /// <summary>
        /// Gets or sets the number of inner corners per chessboard row and column.
        /// </summary>
        [Description("The number of inner corners per chessboard row and column.")]
        public Size PatternSize { get; set; }

        /// <summary>
        /// Gets or sets a value specifying the available operation flags for
        /// finding chessboard corners.
        /// </summary>
        [Description("Specifies the available operation flags for finding chessboard corners.")]
        public ChessboardCalibrationFlags CalibrationFlags { get; set; }

        /// <summary>
        /// Finds the positions of internal corners for each chessboard image in
        /// an observable sequence.
        /// </summary>
        /// <param name="source">
        /// The sequence of chessboard images for which to find the internal
        /// corner positions.
        /// </param>
        /// <returns>
        /// A sequence of <see cref="KeyPointCollection"/> objects representing the
        /// positions of internal corners detected in each chessboard image.
        /// </returns>
        public override IObservable<KeyPointCollection> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                var patternSize = PatternSize;
                var corners = new Point2f[patternSize.Width * patternSize.Height];
                CV.FindChessboardCorners(input, patternSize, corners, out int cornerCount, CalibrationFlags);
                var result = new KeyPointCollection(input);
                for (int i = 0; i < cornerCount; i++)
                {
                    result.Add(corners[i]);
                }
                return result;
            });
        }
    }
}
