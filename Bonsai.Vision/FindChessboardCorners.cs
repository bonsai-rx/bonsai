using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Vision
{
    [Description("Finds the positions of internal corners in a chessboard image.")]
    public class FindChessboardCorners : Transform<IplImage, KeyPointCollection>
    {
        public FindChessboardCorners()
        {
            PatternSize = new Size(9, 7);
            CalibrationFlags = ChessboardCalibrationFlags.AdaptiveThreshold
                | ChessboardCalibrationFlags.NormalizeImage
                | ChessboardCalibrationFlags.FastCheck;
        }

        [Description("The number of inner corners per chessboard row and column.")]
        public Size PatternSize { get; set; }

        [Description("The available operation flags for finding chessboard corners.")]
        public ChessboardCalibrationFlags CalibrationFlags { get; set; }

        public override IObservable<KeyPointCollection> Process(IObservable<IplImage> source)
        {
            return source.Select(input =>
            {
                int cornerCount;
                var patternSize = PatternSize;
                var corners = new Point2f[patternSize.Width * patternSize.Height];
                CV.FindChessboardCorners(input, patternSize, corners, out cornerCount, CalibrationFlags);
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
