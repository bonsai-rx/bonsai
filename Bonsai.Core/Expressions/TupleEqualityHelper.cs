using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Bonsai.Expressions
{
    internal static class TupleEqualityHelper
    {
        static readonly HashSet<Type> TupleTypeDefinitions = new HashSet<Type>
        {
            typeof(Tuple<>), typeof(Tuple<,>), typeof(Tuple<,,>), typeof(Tuple<,,,>),
            typeof(Tuple<,,,,>), typeof(Tuple<,,,,,>), typeof(Tuple<,,,,,,>),
            typeof(ValueTuple<>), typeof(ValueTuple<,>), typeof(ValueTuple<,,>), typeof(ValueTuple<,,,>),
            typeof(ValueTuple<,,,,>), typeof(ValueTuple<,,,,,>), typeof(ValueTuple<,,,,,,>)
        };

        public static Expression Equal(Expression left, Expression right)
        {
            return Build(left, right, Expression.Equal, Expression.AndAlso);
        }

        public static Expression NotEqual(Expression left, Expression right)
        {
            return Build(left, right, Expression.NotEqual, Expression.OrElse);
        }

        static Expression Build(
            Expression left,
            Expression right,
            Func<Expression, Expression, Expression> comparison,
            Func<Expression, Expression, Expression> combinator)
        {
            if (left.Type == right.Type && IsTuple(left.Type, out int itemCount))
            {
                Expression result = null;
                for (int i = 1; i <= itemCount; i++)
                {
                    var name = "Item" + i;
                    var itemComparison = Build(
                        Expression.PropertyOrField(left, name),
                        Expression.PropertyOrField(right, name),
                        comparison,
                        combinator);
                    result = result == null ? itemComparison : combinator(result, itemComparison);
                }

                return result;
            }

            return comparison(left, right);
        }

        static bool IsTuple(Type type, out int itemCount)
        {
            if (type.IsGenericType && TupleTypeDefinitions.Contains(type.GetGenericTypeDefinition()))
            {
                itemCount = type.GetGenericArguments().Length;
                return true;
            }

            itemCount = 0;
            return false;
        }
    }
}
