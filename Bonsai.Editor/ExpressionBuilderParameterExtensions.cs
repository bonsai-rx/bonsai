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
        public static int GetEdgeConnectionIndex(this ExpressionBuilderArgument argument)
        {
            var connectionIndexString = argument.Name.Substring(ExpressionBuilderArgument.Source.Length);
            return string.IsNullOrEmpty(connectionIndexString) ? 0 : int.Parse(connectionIndexString);
        }

        public static void IncrementEdgeValue(this ExpressionBuilderArgument argument)
        {
            argument.Name = ExpressionBuilderArgument.Source + (GetEdgeConnectionIndex(argument) + 1);
        }

        public static void DecrementEdgeValue(this ExpressionBuilderArgument argument)
        {
            var connectionIndex = GetEdgeConnectionIndex(argument) - 1;
            argument.Name = ExpressionBuilderArgument.Source;
            argument.Name += connectionIndex;
        }
    }
}
