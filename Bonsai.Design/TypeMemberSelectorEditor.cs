using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bonsai.Expressions;

namespace Bonsai.Design
{
    /// <summary>
    /// Provides a user interface editor that displays a dialog for selecting
    /// members of an operator type.
    /// </summary>
    public class TypeMemberSelectorEditor : MemberSelectorEditor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TypeMemberSelectorEditor"/> class.
        /// </summary>
        TypeMemberSelectorEditor() : base(false, false)
        { }


    }
}
