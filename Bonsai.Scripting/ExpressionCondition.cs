using Bonsai.Expressions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Scripting
{
    [WorkflowElementCategory(ElementCategory.Condition)]
    [Description("An expression that is used to filter individual elements of the input sequence.")]
    public class ExpressionCondition : SingleArgumentExpressionBuilder
    {
        static readonly MethodInfo whereMethod = typeof(Observable).GetMethods()
                                                                   .Single(m => m.Name == "Where" &&
                                                                           m.GetParameters().Length == 2 &&
                                                                           m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public ExpressionCondition()
        {
            Expression = "it";
        }

        [Editor(DesignTypes.MultilineStringEditor, typeof(UITypeEditor))]
        [Description("The expression that determines which elements to filter.")]
        public string Expression { get; set; }

        public override Expression Build(IEnumerable<Expression> arguments)
        {
            var source = arguments.First();
            var sourceType = source.Type.GetGenericArguments()[0];
            var predicate = global::System.Linq.Dynamic.DynamicExpression.ParseLambda(sourceType, typeof(bool), Expression);
            return System.Linq.Expressions.Expression.Call(whereMethod.MakeGenericMethod(sourceType), source, predicate);
        }
    }
}
