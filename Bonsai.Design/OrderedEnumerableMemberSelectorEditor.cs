using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

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
