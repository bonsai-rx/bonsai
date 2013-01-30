using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Bonsai
{
    public static class ExpressionHelper
    {
        public const string MemberSeparator = ".";

        public static Expression MemberAccess(Expression expression, string memberPath)
        {
            if (!string.IsNullOrWhiteSpace(memberPath))
            {
                foreach (var memberName in memberPath.Split(new[] { MemberSeparator }, StringSplitOptions.RemoveEmptyEntries))
                {
                    expression = Expression.PropertyOrField(expression, memberName);
                }
            }

            return expression;
        }
    }
}
