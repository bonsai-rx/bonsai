using NuGet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.NuGet
{
    static class QueryHelper
    {
        const string TagsProperty = "Tags";
        static readonly MethodInfo stringConcat = typeof(string).GetMethod("Concat", new Type[] { typeof(string), typeof(string) });
        static readonly MethodInfo stringContains = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        static readonly MethodInfo stringToLower = typeof(string).GetMethod("ToLower", Type.EmptyTypes);

        public static IQueryable<T> WithTags<T>(this IQueryable<T> packages, IEnumerable<string> tags) where T : IPackage
        {
            if (!tags.Any())
            {
                return packages;
            }

            var package = Expression.Parameter(typeof(IPackageMetadata));
            var property = Expression.Property(package, TagsProperty);
            var condition = (from term in tags
                             select BuildTagExpression(property, term))
                             .Aggregate(Expression.AndAlso);

            // Check that the tag string is not null
            var notNull = Expression.NotEqual(property, Expression.Constant(null));
            condition = Expression.AndAlso(notNull, condition);

            var predicate = Expression.Lambda<Func<T, bool>>(condition, package);
            return packages.Where(predicate);
        }

        static Expression BuildTagExpression(MemberExpression property, string term)
        {
            // Pad both the tag string and the search term with white space to ensure an exact match
            var paddedLowercaseTags = Expression.Call(property, stringToLower);
            paddedLowercaseTags = Expression.Call(stringConcat, Expression.Constant(" "), paddedLowercaseTags);
            paddedLowercaseTags = Expression.Call(stringConcat, paddedLowercaseTags, Expression.Constant(" "));

            term = " " + term + " ";
            return Expression.Call(paddedLowercaseTags, stringContains, Expression.Constant(term.ToLower()));
        }
    }
}
