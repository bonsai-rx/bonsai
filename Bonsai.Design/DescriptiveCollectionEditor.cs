using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace Bonsai.Design
{
    public class DescriptiveCollectionEditor : CollectionEditor
    {
        public DescriptiveCollectionEditor(Type type)
            : base(type)
        {
        }

        protected override CollectionForm CreateCollectionForm()
        {
            var form = base.CreateCollectionForm();
            form.Shown += delegate { ActivateDescription(form); };
            return form;
        }

        static void ActivateDescription(Control control)
        {
            var propertyGrid = control as PropertyGrid;
            if (propertyGrid != null)
            {
                propertyGrid.HelpVisible = true;
            }

            foreach (Control child in control.Controls)
            {
                ActivateDescription(child);
            }
        }
    }
}
