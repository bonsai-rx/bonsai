using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;

namespace Bonsai.Expressions
{
    class PublishScope
    {
        readonly List<ExpressionBuilder> references = new List<ExpressionBuilder>();
        static readonly MethodInfo publishMethod = typeof(Observable).GetMethods()
                                                                     .Single(m => m.Name == "Publish" &&
                                                                             m.GetParameters().Length == 2 &&
                                                                             !m.GetParameters()[1].ParameterType.IsGenericParameter &&
                                                                             m.GetParameters()[1].ParameterType.GetGenericTypeDefinition() == typeof(Func<,>));

        public PublishScope(Expression source)
        {
            Source = source;
            PublishedSource = Expression.Parameter(source.Type);
        }

        public Expression Source { get; private set; }

        public ParameterExpression PublishedSource { get; private set; }

        public List<ExpressionBuilder> References
        {
            get { return references; }
        }

        public Expression Close(Expression result)
        {
            var sourceType = Source.Type.GetGenericArguments()[0];
            var resultType = result.Type.GetGenericArguments()[0];
            var selector = Expression.Lambda(result, PublishedSource);
            return Expression.Call(publishMethod.MakeGenericMethod(sourceType, resultType), Source, selector);
        }
    }
}
