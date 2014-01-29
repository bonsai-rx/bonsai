using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;

namespace Bonsai
{
    /// <summary>
    /// Provides a set of static methods to manipulate expression trees.
    /// </summary>
    public static class ExpressionHelper
    {
        public const string MemberSeparator = ".";
        const string IndexBegin = "[";
        const string IndexEnd = "]";
        const string IndexParameterSeparator = ",";

        /// <summary>
        /// Creates an <see cref="Expression"/> representing a chained access to a member
        /// variable.
        /// </summary>
        /// <param name="instance">The object to which the member chain belongs.</param>
        /// <param name="memberPath">
        /// The path to an inner member variable, separated by a dot. Indexed accessors
        /// are also allowed.
        /// </param>
        /// <returns>The created <see cref="Expression"/>.</returns>
        public static Expression MemberAccess(Expression instance, string memberPath)
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
                        var propertyInfo = instance.Type.GetProperty(propertyName);
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
                        instance = Expression.Property(instance, propertyName, arguments);
                    }
                    else instance = Expression.PropertyOrField(instance, memberName);
                }
            }

            return instance;
        }
    }
}
