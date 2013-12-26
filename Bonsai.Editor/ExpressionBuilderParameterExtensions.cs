using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    static class ExpressionBuilderParameterExtensions
    {
        public static int GetEdgeConnectionIndex(this ExpressionBuilderParameter parameter)
        {
            var connectionIndexString = parameter.Value.Substring(ExpressionBuilderParameter.Source.Length);
            return string.IsNullOrEmpty(connectionIndexString) ? 0 : int.Parse(connectionIndexString);
        }

        public static void IncrementEdgeValue(this ExpressionBuilderParameter parameter)
        {
            parameter.Value = ExpressionBuilderParameter.Source + (GetEdgeConnectionIndex(parameter) + 1);
        }

        public static void DecrementEdgeValue(this ExpressionBuilderParameter parameter)
        {
            var connectionIndex = GetEdgeConnectionIndex(parameter) - 1;
            parameter.Value = ExpressionBuilderParameter.Source;
            parameter.Value += connectionIndex;
        }
    }
}
