using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Bonsai.Expressions;
using OpenCV.Net;
using System.Reactive.Linq;
using System.Reactive.Disposables;

namespace Bonsai.Dsp
{
    public abstract class WindowStatistics<TArray> : Combinator<IObservable<TArray>, IObservable<TArray>> where TArray : Arr
    {
        public ProjectionType ProjectionType { get; set; }

        protected abstract TArray CreateArray(TArray source, Depth depth);

        public override IObservable<IObservable<TArray>> Process(IObservable<IObservable<TArray>> source)
        {
            return Observable.Defer(() =>
            {
                int n = 0;
                TArray temp = null;
                TArray temp2 = null;
                var projectionType = ProjectionType;
                var accumulatorBuffer = new List<TArray>();
                var accumulatorBufferSquared = new List<TArray>();
                return Observable.Using(
                    () => Disposable.Create(() =>
                    {
                        if (temp != null) temp.Close();
                        if (temp2 != null) temp2.Close();
                        accumulatorBuffer.ForEach(image => image.Close());
                        accumulatorBufferSquared.ForEach(image => image.Close());
                    }),
                    resource =>
                    {
                        return source.Do(xs => n++).Select(
                            (window, i) => window.Select(
                                (frame, j) =>
                                {
                                    TArray accumulator;
                                    if (i == 0)
                                    {
                                        if (projectionType == ProjectionType.Min || projectionType == ProjectionType.Max)
                                        {
                                            accumulator = CreateArray(frame, 0);
                                        }
                                        else accumulator = CreateArray(frame, Depth.F32);

                                        CV.Convert(frame, accumulator);
                                        accumulatorBuffer.Add(accumulator);
                                        if (projectionType == ProjectionType.StandardDeviation)
                                        {
                                            var accumulatorSquared = CreateArray(frame, Depth.F32);
                                            accumulatorSquared.SetZero();
                                            accumulatorBufferSquared.Add(accumulatorSquared);
                                        }
                                    }
                                    else
                                    {
                                        accumulator = accumulatorBuffer[j];
                                        switch (projectionType)
                                        {
                                            case ProjectionType.Average:
                                            case ProjectionType.StandardDeviation:
                                                // Ak = Ak-1 + (xk - Ak-1) / k
                                                temp = temp ?? CreateArray(accumulator, 0);
                                                CV.Sub(frame, accumulator, temp); // temp <- xk - Ak-1
                                                CV.ScaleAdd(temp, Scalar.All(1.0 / n), accumulator, accumulator); // Ak-1 + temp / k
                                                if (projectionType == ProjectionType.StandardDeviation)
                                                {
                                                    // Qk = Qk-1 + (xk - Ak-1)*(xk - Ak)
                                                    var accumulatorSquared = accumulatorBufferSquared[j];
                                                    temp2 = temp2 ?? CreateArray(accumulator, 0);
                                                    CV.Sub(frame, accumulator, temp2); // xk - Ak
                                                    CV.Mul(temp, temp2, temp, 1); // temp <- (xk - Ak-1)*(xk - Ak)
                                                    CV.Add(accumulatorSquared, temp, accumulatorSquared); // Qk-1 + temp
                                                }
                                                break;
                                            case ProjectionType.Min:
                                                CV.Min(accumulator, frame, accumulator);
                                                break;
                                            case ProjectionType.Max:
                                                CV.Max(accumulator, frame, accumulator);
                                                break;
                                            case ProjectionType.Sum:
                                                CV.Add(accumulator, frame, accumulator);
                                                break;
                                            default:
                                                throw new InvalidOperationException("Specified projection type is invalid.");
                                        }
                                    }

                                    var result = CreateArray(accumulator, 0);
                                    CV.Copy(accumulator, result);
                                    if (projectionType == ProjectionType.StandardDeviation)
                                    {
                                        CV.ConvertScale(result, result, 1.0 / (n - 1), 0); // s2 <- Qn / n-1
                                        CV.Pow(result, result, 0.5); // s <- sqrt(s2)
                                    }
                                    return result;
                                }).Publish().RefCount());
                    });
            });
        }
    }

    public class WindowStatistics : WindowStatistics<Mat>
    {
        protected override Mat CreateArray(Mat source, Depth depth)
        {
            depth = depth > 0 ? depth : source.Depth;
            return new Mat(source.Rows, source.Cols, depth, source.Channels);
        }
    }

    public enum ProjectionType
    {
        Average,
        Min,
        Max,
        Sum,
        StandardDeviation
    }
}
