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
    [Description("Splits an array into multiple sub-arrays along the specified dimension.")]
    public class Slice
    {
        [Description("The dimension along which to slice the array.")]
        public int Axis { get; set; }

        [Description("The number of elements in each slice.")]
        public int Count { get; set; }

        [Description("The optional number of elements to skip between the creation of each slice.")]
        public int? Skip { get; set; }

        static IEnumerable<IplImage> SliceRows(IplImage input, int count, int skip)
        {
            for (int i = 0; i <= input.Height - skip; i += skip)
            {
                var rectangle = new Rect(0, i, input.Width, count);
                yield return input.GetSubRect(rectangle);
            }
        }

        static IEnumerable<IplImage> SliceCols(IplImage input, int count, int skip)
        {
            for (int i = 0; i <= input.Width - skip; i += skip)
            {
                var rectangle = new Rect(i, 0, count, input.Height);
                yield return input.GetSubRect(rectangle);
            }
        }

        static IEnumerable<Mat> SliceRows(Mat input, int count, int skip)
        {
            for (int i = 0; i <= input.Rows - skip; i += skip)
            {
                var rectangle = new Rect(0, i, input.Cols, count);
                yield return input.GetSubRect(rectangle);
            }
        }

        static IEnumerable<Mat> SliceCols(Mat input, int count, int skip)
        {
            for (int i = 0; i <= input.Cols - skip; i += skip)
            {
                var rectangle = new Rect(i, 0, count, input.Rows);
                yield return input.GetSubRect(rectangle);
            }
        }

        public IObservable<IplImage> Process(IObservable<IplImage> source)
        {
            return source.SelectMany(input =>
            {
                var axis = Axis;
                var count = Count;
                var skip = Skip.GetValueOrDefault(count);
                return axis == 0 ? SliceRows(input, count, skip) : SliceCols(input, count, skip);
            });
        }

        public IObservable<Mat> Process(IObservable<Mat> source)
        {
            return source.SelectMany(input =>
            {
                var axis = Axis;
                var count = Count;
                var skip = Skip.GetValueOrDefault(count);
                return axis == 0 ? SliceRows(input, count, skip) : SliceCols(input, count, skip);
            });
        }
    }
}
