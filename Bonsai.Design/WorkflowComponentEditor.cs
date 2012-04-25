using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public abstract class WorkflowComponentEditor : ComponentEditor
    {
        public bool EditComponent(object component, IServiceProvider provider, IWin32Window owner)
        {
            return EditComponent(null, component, provider, owner);
        }

        public override bool EditComponent(ITypeDescriptorContext context, object component)
        {
            return EditComponent(context, component, null, null);
        }

        public abstract bool EditComponent(ITypeDescriptorContext context, object component, IServiceProvider provider, IWin32Window owner);
    }
}
