using System;
using System.Linq;
using System.Linq.Expressions;

namespace Bonsai.Design
{
    class OrderedEnumerableMemberSelectorEditor : MemberSelectorEditor
    {
        public OrderedEnumerableMemberSelectorEditor()
            : base(GetEnumerableElementType, true)
        {
        }

        static Type GetEnumerableElementType(Expression expression)
        {
            var parameterType = expression.Type.GetGenericArguments()[0];
            return ExpressionHelper.GetGenericTypeBindings(typeof(IOrderedEnumerable<>), parameterType).FirstOrDefault();
        }
    }
}
