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
    public abstract class WindowStatistics<TArray> : Combinator<IObservable<TArray>, IObservable<TArray>> where TArray : CvArr
    {
        public ProjectionType ProjectionType { get; set; }

        protected abstract TArray CreateArray(TArray source, CvMatDepth depth);

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
                                        else accumulator = CreateArray(frame, CvMatDepth.CV_32F);

                                        Core.cvConvert(frame, accumulator);
                                        accumulatorBuffer.Add(accumulator);
                                        if (projectionType == ProjectionType.StandardDeviation)
                                        {
                                            var accumulatorSquared = CreateArray(frame, CvMatDepth.CV_32F);
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
                                                Core.cvSub(frame, accumulator, temp, CvArr.Null); // temp <- xk - Ak-1
                                                Core.cvScaleAdd(temp, CvScalar.All(1.0 / n), accumulator, accumulator); // Ak-1 + temp / k
                                                if (projectionType == ProjectionType.StandardDeviation)
                                                {
                                                    // Qk = Qk-1 + (xk - Ak-1)*(xk - Ak)
                                                    var accumulatorSquared = accumulatorBufferSquared[j];
                                                    temp2 = temp2 ?? CreateArray(accumulator, 0);
                                                    Core.cvSub(frame, accumulator, temp2, CvArr.Null); // xk - Ak
                                                    Core.cvMul(temp, temp2, temp, 1); // temp <- (xk - Ak-1)*(xk - Ak)
                                                    Core.cvAdd(accumulatorSquared, temp, accumulatorSquared, CvArr.Null); // Qk-1 + temp
                                                }
                                                break;
                                            case ProjectionType.Min:
                                                Core.cvMin(accumulator, frame, accumulator);
                                                break;
                                            case ProjectionType.Max:
                                                Core.cvMax(accumulator, frame, accumulator);
                                                break;
                                            case ProjectionType.Sum:
                                                Core.cvAdd(accumulator, frame, accumulator, CvArr.Null);
                                                break;
                                            default:
                                                throw new InvalidOperationException("Specified projection type is invalid.");
                                        }
                                    }

                                    var result = CreateArray(accumulator, 0);
                                    Core.cvCopy(accumulator, result);
                                    if (projectionType == ProjectionType.StandardDeviation)
                                    {
                                        Core.cvConvertScale(result, result, 1.0 / (n - 1), 0); // s2 <- Qn / n-1
                                        Core.cvPow(result, result, 0.5); // s <- sqrt(s2)
                                    }
                                    return result;
                                }).Publish().RefCount());
                    });
            });
        }
    }

    public class WindowStatistics : WindowStatistics<CvMat>
    {
        protected override CvMat CreateArray(CvMat source, CvMatDepth depth)
        {
            depth = depth > 0 ? depth : source.Depth;
            return new CvMat(source.Rows, source.Cols, depth, source.NumChannels);
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
