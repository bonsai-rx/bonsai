using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Bonsai.Design
{
    internal interface IExtendedVisitableType
    {
        IEnumerable<MemberInfo> GetExtendedMembers();
        MemberInfo GetExtendedMember(string name);
    }
}
