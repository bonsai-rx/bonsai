using Bonsai.Expressions;
using OpenCV.Net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;

namespace Bonsai.Dsp
{
    /// <summary>
    /// Represents an operator that converts each array or collection in the sequence into
    /// a managed array.
    /// </summary>
    [WorkflowElementCategory(ElementCategory.Transform)]
    [Description("Converts each array or collection in the sequence into a managed array.")]
    public class ConvertToArray : SelectBuilder
    {
        /// <summary>
        /// Gets or sets the bit depth of each element in the input array.
        /// </summary>
        /// <remarks>
        /// If this property is not specified, the default depth will be automatically
        /// selected based on the type of the input array elements.
        /// </remarks>
        [TypeConverter(typeof(DepthConverter))]
        [Description("The bit depth of each element in the input array.")]
        public Depth? Depth { get; set; }


        /// <summary>
        /// Returns the expression that maps the array or collection into
        /// a managed array.
        /// </summary>
        /// <inheritdoc/>
        protected override Expression BuildSelector(Expression expression)
        {
            var depth = Depth;
            var arrayType = expression.Type;
            if (depth.HasValue)
            {
                var elementType = GetElementType(depth.Value);
                var depthExpression = Expression.Constant(depth.Value);
                return Expression.Call(typeof(ConvertToArray), nameof(Process), new[] { arrayType, elementType }, expression, depthExpression);
            }
            else if (!typeof(Arr).IsAssignableFrom(arrayType))
            {
                var elementType = ExpressionHelper.GetGenericTypeBindings(typeof(IEnumerable<>), expression.Type).FirstOrDefault();
                if (elementType == null) throw new InvalidOperationException("The input buffer must be an array or enumerable type.");
                return Expression.Call(typeof(Enumerable), nameof(Enumerable.ToArray), new[] { elementType }, expression);
            }
            else return Expression.Call(typeof(ConvertToArray), nameof(Process), new[] { arrayType }, expression);
        }

        static Type GetElementType(Depth depth)
        {
            switch (depth)
            {
                case OpenCV.Net.Depth.U16: return typeof(ushort);
                case OpenCV.Net.Depth.S16: return typeof(short);
                case OpenCV.Net.Depth.S32: return typeof(int);
                case OpenCV.Net.Depth.F32: return typeof(float);
                case OpenCV.Net.Depth.F64: return typeof(double);
                case OpenCV.Net.Depth.U8:
                case OpenCV.Net.Depth.S8:
                case OpenCV.Net.Depth.UserType:
                default:
                    return typeof(byte);
            }
        }

        static byte[] Process<TArray>(TArray input) where TArray : Arr
        {
            var inputHeader = input.GetMat();
            return ArrHelper.ToArray(inputHeader);
        }

        static TElement[] Process<TArray, TElement>(TArray input, Depth depth)
            where TArray : Arr
            where TElement : struct
        {
            var inputHeader = input.GetMat();
            var output = new TElement[inputHeader.Rows * inputHeader.Cols];
            using (var outputHeader = Mat.CreateMatHeader(output, inputHeader.Rows, inputHeader.Cols, depth, 1))
            {
                CV.Convert(inputHeader, outputHeader);
            }
            return output;
        }
    }
}
