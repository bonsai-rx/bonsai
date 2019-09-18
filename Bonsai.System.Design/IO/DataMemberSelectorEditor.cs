using Bonsai.Design;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.IO.Design
{
    class DataMemberSelectorEditor : MemberSelectorEditor
    {
        public DataMemberSelectorEditor()
            : base(GetDataElementType, true)
        {
        }

        static Type GetDataElementType(Expression expression)
        {
            var parameterType = expression.Type.GetGenericArguments()[0];
            return ExpressionHelper.GetGenericTypeBindings(typeof(IList<>), parameterType).FirstOrDefault() ?? parameterType;
        }
    }
}
