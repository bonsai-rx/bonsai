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
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Calculates the absolute array norm, absolute difference norm, or relative difference norm.")]
    public class Norm
    {
        public Norm()
        {
            NormType = NormTypes.L2;
        }

        [Description("The type of array norm to calculate.")]
        public NormTypes NormType { get; set; }

        static double ComputeNorm(double x, double y, NormTypes norm)
        {
            switch (norm)
            {
                case NormTypes.C: return Math.Max(Math.Abs(x), Math.Abs(y));
                case NormTypes.L1: return Math.Abs(x) + Math.Abs(y);
                case NormTypes.L2: return Math.Sqrt(x * x + y * y);
                case NormTypes.L2Sqr: return x * x + y * y;
                default: throw new InvalidOperationException("The specified norm is not supported for this data type.");
            }
        }

        static double ComputeNorm(double x, double y, double z, NormTypes norm)
        {
            switch (norm)
            {
                case NormTypes.C: return Math.Max(Math.Abs(x), Math.Max(Math.Abs(y), Math.Abs(z)));
                case NormTypes.L1: return Math.Abs(x) + Math.Abs(y) + Math.Abs(z);
                case NormTypes.L2: return Math.Sqrt(x * x + y * y + z * z);
                case NormTypes.L2Sqr: return x * x + y * y + z * z;
                default: throw new InvalidOperationException("The specified norm is not supported for this data type.");
            }
        }

        public IObservable<double> Process(IObservable<Point> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, NormType));
        }

        public IObservable<double> Process(IObservable<Point2f> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, NormType));
        }

        public IObservable<double> Process(IObservable<Point2d> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, NormType));
        }

        public IObservable<double> Process(IObservable<Point3f> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, input.Z, NormType));
        }

        public IObservable<double> Process(IObservable<Point3d> source)
        {
            return source.Select(input => ComputeNorm(input.X, input.Y, input.Z, NormType));
        }

        public IObservable<double> Process<TArray>(IObservable<TArray> source) where TArray : Arr
        {
            return source.Select(input => CV.Norm(input, null, NormType));
        }

        public IObservable<double> Process<TArray1, TArray2>(IObservable<Tuple<TArray1, TArray2>> source)
            where TArray1 : Arr
            where TArray2 : Arr
        {
            return source.Select(input => CV.Norm(input.Item1, input.Item2, NormType));
        }

        public IObservable<double> Process<TArray1, TArray2, TMask>(IObservable<Tuple<TArray1, TArray2, TMask>> source)
            where TArray1 : Arr
            where TArray2 : Arr
            where TMask : Arr
        {
            return source.Select(input => CV.Norm(input.Item1, input.Item2, NormType, input.Item3));
        }
    }
}
