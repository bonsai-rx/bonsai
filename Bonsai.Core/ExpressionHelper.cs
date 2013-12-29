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
        public const string IndexBegin = "[";
        public const string IndexEnd = "]";
        public const string IndexParameterSeparator = ",";

        public static Expression MemberAccess(Expression expression, string memberPath)
        {
            if (!string.IsNullOrWhiteSpace(memberPath))
            {
                foreach (var memberName in memberPath.Split(new[] { MemberSeparator }, StringSplitOptions.RemoveEmptyEntries))
                {
                    var indexBegin = memberName.IndexOf(IndexBegin);
                    if (indexBegin >= 0)
                    {
                        var indexEnd = memberName.IndexOf(IndexEnd);
                        if (indexEnd < 0) throw new ArgumentException("Member path has badly formatted index accessor.", "memberPath");
                        var propertyName = memberName.Substring(0, indexBegin);
                        var propertyInfo = expression.Type.GetProperty(propertyName);
                        if (propertyInfo == null) throw new ArgumentException("Member path has reference to non-existent indexed property.", "memberPath");
                        var parameterInfo = propertyInfo.GetIndexParameters();
                        var indexParameters = memberName
                            .Substring(indexBegin + 1, indexEnd - indexBegin - 1)
                            .Split(new[] { IndexParameterSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        var arguments = (from indexParameter in parameterInfo.Zip(indexParameters, (xs, ys) => Tuple.Create(xs, ys))
                                         let parameterType = indexParameter.Item1.ParameterType
                                         let parameter = Convert.ChangeType(indexParameter.Item2, parameterType)
                                         select Expression.Constant(parameter))
                                        .ToArray();
                        expression = Expression.Property(expression, propertyName, arguments);
                    }
                    else expression = Expression.PropertyOrField(expression, memberName);
                }
            }

            return expression;
        }
    }
}
