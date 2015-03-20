using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using Bonsai.Expressions;
using Bonsai.Dag;
using System.Collections.ObjectModel;
using System.Linq.Expressions;

namespace Bonsai.Design
{
    public class MultiMemberSelectorEditor : MemberSelectorEditor
    {
        public MultiMemberSelectorEditor()
            : base(true)
        {
        }
    }
}
